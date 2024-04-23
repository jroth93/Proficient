using Autodesk.Revit.UI.Selection;
using Proficient.Forms;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class ElementPlacer : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = revit.Application.ActiveUIDocument.Document;
        var view = uiDoc.ActiveView;
        var prompt = new Follower(revit);
        

        bool prevErr = false;
        Curve pathCrv;

        while (true)
        {
            try
            {
                prompt.Show();
                prompt.lbl1.Content = prevErr ? "Invalid Path Selection.\nPlease Try Again.\n\nPick Path Line." : "Pick Path Line";


                var cRef = uiDoc.Selection.PickObject(ObjectType.Element, "Pick path line");
                if(doc.GetElement(cRef).Location is LocationCurve lc && lc.Curve.IsBound)
                {
                    pathCrv = lc.Curve;
                }
                else
                {
                    prevErr = true;
                    continue;
                }
                break;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                prompt.Close();
                return Result.Cancelled;
            }
        }

        bool hosted;
        FamilySymbol fs;
        FamilyInstance fi;
        prevErr = false;

        while (true)
        {
            try
            {
                prompt.Show();
                prompt.lbl1.Content = prevErr ? "Invalid Element Selection.\nPlease Try Again\n\nPick Element To Be Placed." : "Pick Element To Be Placed.";

                var elRef = uiDoc.Selection.PickObject(ObjectType.Element, "Pick element to be placed");
                fi = doc.GetElement(elRef) as FamilyInstance;
                if (fi != null)
                {
                    fs = fi.Symbol;
                    hosted = fi.Host != null;
                }
                else
                {
                    prevErr = true;
                    continue;
                }
                break;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                prompt.Close(); 
                return Result.Cancelled;
            }
        }

        prompt.Close();

        var pef = new PlaceElFrm();
        bool placeByNumber;
        double offset = 0;
        double usrIn;
        while (true)
        {
            if (pef.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return Result.Cancelled; 
            placeByNumber = pef.radionumber.Checked;
            try
            {
                usrIn = Convert.ToDouble(pef.textBox1.Text);
                if (placeByNumber)
                    offset = Convert.ToDouble(pef.startoffset.Text);
                break;
            }
            catch (FormatException)
            {
                new TaskDialog("Invalid Form Entry")
                    {
                        MainContent = "Invalid entry. Please try again."
                    }
                    .Show();
            }
        }

        pef.Close();

        var step = placeByNumber ? pathCrv.Length / (usrIn - 1) : usrIn;

        var dist = offset == 0 ? 0 : step - offset;

        var tess = new List<XYZ>();
        var deriv = new List<XYZ>();
        var pts = new List<XYZ>();
        var newDir = new List<XYZ>();

        if (pathCrv as Line != null)
        {
            if (placeByNumber)
            {
                for (var i = 0; i < usrIn; i++)
                {
                    pts.Add(pathCrv.Evaluate(i * step / pathCrv.Length, true));
                    newDir.Add(pathCrv.ComputeDerivatives(i * step / pathCrv.Length, true).get_Basis(0));
                }
            }
            else
            {
                var curLen = offset;
                while (curLen < pathCrv.Length)
                {
                    pts.Add(pathCrv.Evaluate(curLen / pathCrv.Length, true));
                    newDir.Add(pathCrv.ComputeDerivatives(curLen / pathCrv.Length, true).get_Basis(0));
                    curLen += step;
                }

            }
        }
        else
        {
            var curPar = pathCrv.GetEndParameter(0);
            while (curPar <= pathCrv.GetEndParameter(1))
            {
                tess.Add(pathCrv.Evaluate(curPar, false));
                deriv.Add(pathCrv.ComputeDerivatives(curPar, false).get_Basis(0));
                curPar += 0.001;
            }

            var p = pathCrv.GetEndPoint(0);

            foreach (var q in tess)
            {
                if (0 == pts.Count && 0 == offset)
                {
                    pts.Add(p);
                    dist = 0.0;
                    newDir.Add(deriv[0]);
                }
                else
                {
                    dist += p.DistanceTo(q);
                    if (dist >= step)
                    {
                        pts.Add(q);
                        newDir.Add(deriv[tess.IndexOf(q)]);
                        dist = 0;
                    }
                    else if (dist > step)
                    {
                        pts.Add((p + q) / 2);
                        dist = 0;
                        newDir.Add(deriv[tess.IndexOf(q)]);
                    }
                    p = q;
                }

                if (!placeByNumber || pts.Count != Convert.ToInt32(usrIn - 1)) 
                    continue;

                pts.Add(tess.Last());
                newDir.Add(deriv.Last());
                break;
            }
        }

        using var tx = new Transaction(doc, "Place Elements");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;
        
        foreach (var pt in pts)
        {
            var newEl = hosted ?
                doc.Create.NewFamilyInstance(fi.HostFace ?? new Reference(fi.Host), pt, new XYZ(1, 0, 0), fs) : 
                (fi.ViewSpecific ?
                    doc.Create.NewFamilyInstance(pt, fs, view) :
                    doc.Create.NewFamilyInstance(pt, fs, view.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural));

            var rotAx = Line.CreateBound(pt, pt.Add(XYZ.BasisZ));
            var rotAng = newDir[pts.IndexOf(pt)].Y < 0 ? -XYZ.BasisX.AngleTo(newDir[pts.IndexOf(pt)]) : XYZ.BasisX.AngleTo(newDir[pts.IndexOf(pt)]);
            ElementTransformUtils.RotateElement(doc, newEl.Id, rotAx, rotAng);

            foreach (Parameter par in newEl.Parameters)
            {
                var elPar = fi.LookupParameter(par.Definition.Name);
                if (!par.IsReadOnly && elPar.HasValue && par.Definition.Name != "Mark")
                {
                    switch (par.StorageType)
                    {
                        case StorageType.Integer:
                            par.Set(elPar.AsInteger());
                            break;
                        case StorageType.Double:
                            par.Set(elPar.AsDouble());
                            break;
                        case StorageType.String:
                            par.Set(elPar.AsString());
                            break;
                        case StorageType.ElementId:
                            par.Set(elPar.AsElementId());
                            break;
                    }
                }
            }
        }
            
        tx.Commit();
        return Result.Succeeded;
    }
}