//
// (C) Copyright 2003-2019 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using Autodesk.Revit.DB.ExternalService;

namespace Proficient.Keynotes;

/// <summary>
/// <para>Derive from the IExternalResourceServer interface to create a server class
/// for providing external resources (files or data) to Revit.  This example server
/// provides both keynote data and Revit links.</para>
/// <para>The external resource framework has been designed so that the end user
/// loads external resources by browsing with an "open file" dialog.  When the user
/// chooses an external resource server, the file dialog will display the contents
/// of the server's "root folder."  From there, the user can navigate through a system
/// of folders and files specified by the IExternalResourceServer implementation.  The
/// files and folders might be the actual contents of a directory tree only accessible
/// by the server, or might be some other logical structure relevant to the resource
/// that is being loaded.</para>
/// <para>To demonstrate the flexibility of the framework, this example server retrieves
/// keynote data from a fictitious database for users in France and Germany, and from
/// files for all other users.  Revit link models are also obtained from files.  To keep
/// the demonstration simple, the "root" folder for file-based resources is assumed to be
/// \SampleResourceServerRoot, located immediately under the directory where the DLL for
/// this project is placed.  See the separate *.rtf file for additional documentation.</para>
/// </summary>
public class ExternalResourceDBServer : IExternalResourceServer
{
    private readonly Guid dbGuid;
    /// <summary>
    /// Default constructor
    /// </summary>
    public ExternalResourceDBServer()
    {
        dbGuid = Guid.NewGuid();
        KnReload.DbId = dbGuid;
    }

    // Methods that must be implemented by a server for any of Revit's external services
    #region IExternalServer Implementation

    /// Indicate which of Revit's external services this server supports.
    /// Servers derived from IExternalResourceServer *must* return
    /// ExternalServices.BuiltInExternalServices.ExternalResourceService.
    public ExternalServiceId GetServiceId()
    {
        return ExternalServices.BuiltInExternalServices.ExternalResourceService;
    }

    /// Uniquely identifies this server to Revit's ExternalService registry
    public Guid GetServerId()
    {
        return dbGuid;
    }

    /// <summary>
    /// Implement this method to return the name of the server.
    /// </summary>
    public string GetName()
    {
        return "KNServer";
    }


    /// <summary>
    /// # Implement this method to return the id of the vendor of the server.   
    /// </summary>
    public string GetVendorId()
    {
        return "JR";
    }

    /// <summary>
    /// Provide a short description of the server for display to the end user.
    /// </summary>
    public string GetDescription()
    {
        return "KN Resource Server";
    }

    #endregion IExternalServer Implementation

    ///  Methods implemented specifically by servers for the ExternalResource service
    #region IExternalResourceServer Implementation

    public string GetShortName()
    {
        return GetName();
    }

    /// <summary>
    /// Returns a URL address of the provider of this Revit add-in.
    /// </summary>
    public virtual String GetInformationLink()
    {
        return "http://www.autodesk.com";
    }

    /// <summary>
    /// Specify an image to be displayed in browser dialogs when the end user is selecting a resource to load into Revit.
    /// </summary>
    /// <returns>A string containing the full path to an icon file containing 48x48, 32x32 and 16x16 pixel images.</returns>
    public string GetIconPath()
    {
        return string.Empty;
    }

    /// <summary>
    /// IExternalResourceServer classes can support more than one type of external resource.
    /// This one supports keynotes.
    /// </summary>
    public bool SupportsExternalResourceType(ExternalResourceType resourceType)
    {
        return resourceType == ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable;
    }

    /// <summary>
    /// Add keynote resource
    /// </summary>
    public void SetupBrowserData(ExternalResourceBrowserData browserData)
    {
        browserData.AddResource("Keynotes.txt", System.DateTime.Now.ToString());
    }

    /// <summary>
    /// Checks whether the given ExternalResourceReference is formatted correctly for this server.
    /// The format should match one of the formats created in the SetupBrowserData method.
    /// </summary>
    public bool IsResourceWellFormed(ExternalResourceReference extRef)
    {
        return true;
    }

    /// <summary>
    /// Implement this method to compare two ExternalResourceReferences.
    /// </summary>
    /// <param name="referenceInformation_1">The string-string IDictionary of reference information stored in one ExternalResourceReference</param>
    /// <param name="referenceInformation_2">The string-string IDictionary of reference information stored in a second ExternalResourceReference</param>
    /// <returns></returns>
    public virtual bool AreSameResources(IDictionary<string, string> referenceInformation_1, IDictionary<string, string> referenceInformation_2)
    {
        bool same = true;
        if (referenceInformation_1.Count != referenceInformation_2.Count)
        {
            same = false;
        }
        else
        {
            foreach (string key in referenceInformation_1.Keys)
            {
                if (!referenceInformation_2.ContainsKey(key) || referenceInformation_1[key] != referenceInformation_2[key])
                {
                    same = false;
                    break;
                }
            }
        }

        return same;
    }


    /// <summary>
    /// Servers can override the name for UI purposes, but here we just return the names that we
    /// used when we first created the Resources in SetupBrowserData().
    /// </summary>        
    public String GetInSessionPath(ExternalResourceReference err, String savedPath)
    {
        return savedPath;
    }


    /// <summary>
    /// Loads the resources.
    /// </summary>
    /// <param name="loadRequestId">A GUID that uniquely identifies this resource load request from Revit.</param>
    /// <param name="resourceType">The type of external resource that Revit is asking the server to load.</param>
    /// <param name="resourceReference">An ExternalResourceReference identifying which particular resource to load.</param>
    /// <param name="loadContext">Context information, including the name of Revit document that is calling the server,
    /// </param>the resource that is currently loaded and the type of load operation (automatic or user-driven).
    /// <param name="loadContent">An ExternalResourceLoadContent object that will be populated with load data by the
    /// server.  There are different subclasses of ExternalResourceLoadContent for different ExternalResourceTypes.</param>
    public void LoadResource(Guid loadRequestId, ExternalResourceType resourceType, ExternalResourceReference resourceReference, ExternalResourceLoadContext loadContext, ExternalResourceLoadContent loadContent)
    {
        loadContent.LoadStatus = ExternalResourceLoadStatus.Failure;
        loadContent.Version = DateTime.Now.ToString();

        if (loadContent is not KeyBasedTreeEntriesLoadContent kdrlc)
            throw new ArgumentException("Wrong type of ExternalResourceLoadContent (expecting a KeyBasedTreeEntriesLoadContent) for keynote data.", "loadContent");

        kdrlc.Reset();

        foreach (KeynoteEntry kne in knList)
            kdrlc.AddEntry(kne);


        kdrlc.BuildEntries();
        loadContent.LoadStatus = ExternalResourceLoadStatus.Success;
    }

    /// <summary>
    /// Indicates whether the given version of a resource is the most current
    /// version of the data.
    /// </summary>
    /// <param name="extRef">The ExternalResourceReference to check.</param>
    /// <returns>An enum indicating whether the resource is current, out of date, or of unknown status</returns>
    public ResourceVersionStatus GetResourceVersionStatus(ExternalResourceReference extRef)
    {
        // Determine whether currently loaded version is out of date, and return appropriate status.
        String currentlyLoadedVersion = extRef.Version;

        if (currentlyLoadedVersion == String.Empty)
            return ResourceVersionStatus.Unknown;

        return ResourceVersionStatus.OutOfDate;
    }

    /// <summary>
    /// Implement this to extend the base IExternalResourceServer interface with additional methods
    /// that are specific to particular types of external resource (for example, Revit Links).
    /// NOTE: There are no extension methods required for keynote resources.
    /// </summary>
    /// <param name="extensions">An ExternalResourceServerExtensions object that can be populated with
    /// sub-interface objects which can perform operations related to specific types of External Resource.</param>
    public virtual void GetTypeSpecificServerOperations(ExternalResourceServerExtensions extensions)
    {
    }

    #endregion IExternalResourceServer Implementation

    public List<KeynoteEntry> knList = [];
}