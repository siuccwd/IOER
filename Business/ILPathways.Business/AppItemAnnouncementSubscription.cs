/*********************************************************************************
= Author: Jerome Grimmer
=
= Date: Jan 19/2012
= Assembly: workNet.BusObj.Entity
= Description:
= Notes: THIS ITEM NEEDS TO BE MOVED TO ITS OWN FILE!!!!!
=
=
= Copyright 2012, Illinois workNet All rights reserved.
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;


namespace ILPathways.Business
{
  [Serializable]
  public class AppItemAnnouncementSubscription : BaseBusinessDataEntity
  {
    private string _category;
    public string Category
    {
      get { return this._category; }
      set { this._category = value; }
    }

    private int _frequency;
    public int Frequency
    {
      get { return this._frequency; }
      set { this._frequency = value; }
    }

    private string _email;
    public string Email
    {
      get { return this._email; }
      set { this._email = value; }
    }

    private bool _isValidated;
    public bool IsValidated
    {
      get { return this._isValidated; }
      set { this._isValidated = value; }
    }



    public AppItemAnnouncementSubscription()
    {

    }
  }
}
