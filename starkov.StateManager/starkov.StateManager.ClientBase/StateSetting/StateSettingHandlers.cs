using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using starkov.StateManager.StateSetting;

namespace starkov.StateManager
{
  partial class StateSettingClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var isDocType = _obj.EntityType == StateSetting.EntityType.Document;
      
      _obj.State.Properties.EntityTypeGuid.IsVisible = !isDocType;
      _obj.State.Properties.DocType.IsVisible = isDocType;
      _obj.State.Properties.DocType.IsRequired = isDocType;
    }

    public virtual void EntityTypeValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      var isDocType = e.NewValue == StateSetting.EntityType.Document;
      
      _obj.State.Properties.EntityTypeGuid.IsVisible = !isDocType;
      _obj.State.Properties.DocType.IsVisible = isDocType;
      _obj.State.Properties.DocType.IsRequired = isDocType;
      
      if (e.NewValue != e.OldValue && (_obj.PropertySettings.Any() || _obj.PropertyDisplayNames.Any()))
      {
        var dialog = Dialogs.CreateConfirmDialog("При смене типа сущности все свойства будут очищены, продолжить?");
        if (dialog.Show())
        {
          _obj.PropertySettings.Clear();
          _obj.PropertyDisplayNames.Clear();
        }
      }
    }
  }

  partial class StateSettingPropertySettingsClientHandlers
  {

    public virtual void PropertySettingsRequiredCriteriaValueInput(Sungero.Presentation.TextValueInputEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.NewValue) && _obj.Required == StateManager.StateSettingPropertySettings.Required.No)
        _obj.Required = StateManager.StateSettingPropertySettings.Required.Yes;
    }

    public virtual void PropertySettingsEnabledCriteriaValueInput(Sungero.Presentation.TextValueInputEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.NewValue) && _obj.Enabled == StateManager.StateSettingPropertySettings.Enabled.No)
        _obj.Enabled = StateManager.StateSettingPropertySettings.Enabled.Yes;
    }

    public virtual void PropertySettingsVisabilityCriteriaValueInput(Sungero.Presentation.TextValueInputEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.NewValue) && _obj.Visible == StateManager.StateSettingPropertySettings.Visible.No)
        _obj.Visible = StateManager.StateSettingPropertySettings.Visible.Yes;
    }
  }



}