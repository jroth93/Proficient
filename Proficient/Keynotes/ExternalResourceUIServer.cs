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

class ExternalResourceUIServer : IExternalResourceUIServer
{

    // Methods that must be implemented by a server for any of Revit's external services
    #region IExternalServer Implementation
    /// <summary>
    /// Return the Id of the server. 
    /// </summary>
    public System.Guid GetServerId()
    {
        return m_myServerId;
    }

    /// <summary>
    /// Return the Id of the service that the server belongs to. 
    /// </summary>
    public ExternalServiceId GetServiceId()
    {
        return ExternalServices.BuiltInExternalServices.ExternalResourceUIService;
    }

    /// <summary>
    /// Return the server's name. 
    /// </summary>
    public System.String GetName()
    {
        return "KN UI Server";
    }

    /// <summary>
    /// Return the server's vendor Id. 
    /// </summary>
    public System.String GetVendorId()
    {
        return "JR";
    }

    /// <summary>
    /// Return the description of the server. 
    /// </summary>
    public System.String GetDescription()
    {
        return "UI server for the KN resource server";
    }

    #endregion IExternalServer Implementation

    #region IExternalResourceUIServer Interface Implementation

    /// <summary>
    /// Return the Id of the related DB server. 
    /// </summary>
    /// 
    public System.Guid GetDBServerId()
    {
        return m_myDBServerId;
    }


    /// <summary>
    /// Reports the results of loads from the DB server (ExternalResourceServer).
    /// This method should be implemented to provide UI to communicate success or failure
    /// of a particular external resource load operation to the user.
    /// </summary>
    /// <param name="doc">The Revit model into which the External Resource was loaded.
    /// </param>
    /// <param name="loadDataList">Contains a list of ExternalResourceLoadData with results
    /// for all external resources loaded by the DB server.  It is possible for the DB server
    /// to have loaded more than one resource (for example, loading several linked files
    /// when a host file is opened by the user).
    /// </param>
    public void HandleLoadResourceResults(Document doc, IList<ExternalResourceLoadData> loadDataList)
    {

    }


    /// <summary>
    /// Use this method to report any problems that occurred while the user was browsing for External Resources.
    /// Revit will call this method each time the end user browses to a new folder location, or selects an item
    /// and clicks Open.
    /// </summary>
    public void HandleBrowseResult(ExternalResourceUIBrowseResultType resultType, string browsingItemPath)
    {

    }

    #endregion IExternalResourceUIServer Interface Implementation


    #region ExternalResourceUIServer Member Variables

    private static Guid m_myServerId = new Guid("E9B6C194-62DE-4134-900D-BA8DF7AD33FA");
    private static Guid m_myDBServerId = new Guid("5F3CAA13-F073-4F93-BDC2-B7F4B806CDAF");

    #endregion ExternalResourceUIServer Member Variables

}