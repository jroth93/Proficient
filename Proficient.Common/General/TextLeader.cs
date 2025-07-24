namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class TextLeader : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        var pl = uiDoc.Selection.PickPoint();
        var pt = uiDoc.Selection.PickPoint();

        var txtType = new FilteredElementCollector(doc)
            .OfClass(typeof(TextNoteType))
            .Cast<TextNoteType>()
            .FirstOrDefault(t => t.Name == Main.Settings.DefFont);

        if (txtType == null)
            return Result.Failed;

        var txtOpt = new TextNoteOptions(txtType.Id);

        using var tx = new Transaction(doc, "New Text With Leader");
        if (tx.Start() != TransactionStatus.Started) 
            return Result.Failed;

        var newTxt = TextNote.Create(doc, view.Id, pt, 0.05, "TEXT", txtOpt);
        if (pl.X <= pt.X)
        {
            var ldr = newTxt.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
            newTxt.HorizontalAlignment = HorizontalTextAlignment.Left;
            ldr.End = pl;
            ldr.Elbow = new XYZ(ldr.Anchor.X - view.Scale / 96.0, ldr.Anchor.Y, ldr.Anchor.Z);
        }
        else if (pl.X > pt.X)
        {
            var ldr = newTxt.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_R);
            newTxt.HorizontalAlignment = HorizontalTextAlignment.Right;
            ldr.End = pl;
            ldr.Elbow = new XYZ(ldr.Anchor.X + view.Scale / 96.0, ldr.Anchor.Y, ldr.Anchor.Z);
        }
        tx.Commit();

        return Result.Succeeded;

    }
}