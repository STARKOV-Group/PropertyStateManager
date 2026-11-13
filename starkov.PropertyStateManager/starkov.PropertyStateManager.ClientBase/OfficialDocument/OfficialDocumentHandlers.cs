using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using starkov.PropertyStateManager.OfficialDocument;

namespace starkov.PropertyStateManager
{
  partial class OfficialDocumentClientHandlers
  {
    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      Functions.OfficialDocument.SetPropertiesStates(_obj);
    }

  }
}