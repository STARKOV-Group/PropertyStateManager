using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using starkov.StateManager.StateSetting;
using Sungero.Domain.Shared;

namespace starkov.StateManager.Client
{
  partial class StateSettingPropertyDisplayNamesActions
  {
    public virtual bool CanAddDocKindsForDisplayName(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public virtual void AddDocKindsForDisplayName(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      var availableDocKinds = Sungero.Docflow.DocumentKinds.GetAll(s =>
                                                                   Equals(s.DocumentType, _obj.StateSetting.DocType))
        .Where(s => s.Status == Sungero.Docflow.DocumentKind.Status.Active)
        .OrderBy(s => s.Name)
        .ToList();

      if (!availableDocKinds.Any())
      {
        Dialogs.ShowMessage("Для выбранного типа документа нет доступных видов документов",
                            MessageType.Information);
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog("Виды документов.");
      var defaultKindsIds = _obj.DocumentKindsids?.Split(';')?.Select(long.Parse) ?? new List<long>();
      var defaultKinds = Sungero.Docflow.DocumentKinds.GetAll(s => defaultKindsIds.Contains(s.Id)).ToList();
      var dokKins = dialog.AddSelectMany("Виды документов", true, defaultKinds.ToArray()).From(availableDocKinds.ToArray());
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        _obj.DocumentKindsids = string.Join(";", dokKins.Value.Select(s => s.Id));
        _obj.DocKindNames = string.Join(";", dokKins.Value.Select(s => s.DisplayValue));
      }
    }
  }

  internal static class StateSettingPropertyDisplayNamesStaticActions
  {
    public static void AddPropertyForDisplayName(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      var propertyStateSetting = StateSettings.As(e.Entity);
      var propertiesDictionary = new Dictionary<string, Sungero.Metadata.PropertyMetadata>();
      
      var typeGuid = Guid.Empty;
      if (!Guid.TryParse(propertyStateSetting.EntityTypeGuid, out typeGuid))
      {
        Dialogs.ShowMessage("Указан некорректный гуид типа", MessageType.Error);
        return;
      }
      
      var metadata = Sungero.Metadata.Services.MetadataSearcher.FindEntityMetadata(typeGuid);
      foreach (var prop in metadata.Properties.ToList())
        propertiesDictionary.Add($"{prop.GetLocalizedName()} ({prop.Name})", prop);
      
      var dialog = Dialogs.CreateInputDialog("Выбор свойства");
      var property = dialog.AddSelect("Свойство", true, 0).From(propertiesDictionary.Select(s => s.Key).ToArray());
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var item = propertiesDictionary.ElementAt(property.ValueIndex).Value;
        var row = propertyStateSetting.PropertyDisplayNames.AddNew();
        row.LocalisedName = item.GetLocalizedName();
        row.PropertyNameGuid = item.NameGuid.ToString();
        row.PropertyName = item.Name;
      }
    }

    public static bool CanAddPropertyForDisplayName(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

  }

  partial class StateSettingAnyChildEntityActions
  {
    public override void CopyChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.CopyChildEntity(e);
    }

    public override bool CanCopyChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      if (_all is Sungero.Domain.Shared.IChildEntityCollection<StateManager.IStateSettingPropertySettings>)
        return false;
      else if (_all is Sungero.Domain.Shared.IChildEntityCollection<StateManager.IStateSettingPropertyDisplayNames>)
        return false;
      
      return base.CanCopyChildEntity(e);
    }

    public override void AddChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.AddChildEntity(e);
    }

    public override bool CanAddChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      if (_all is Sungero.Domain.Shared.IChildEntityCollection<StateManager.IStateSettingPropertySettings>)
        return false;
      else if (_all is Sungero.Domain.Shared.IChildEntityCollection<StateManager.IStateSettingPropertyDisplayNames>)
        return false;
      
      return base.CanAddChildEntity(e);
    }

  }

  internal static class StateSettingPropertySettingsStaticActions
  {
    public static bool CanAddProperty(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public static void AddProperty(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      var propertyStateSetting = StateSettings.As(e.Entity);
      if (propertyStateSetting.DocType == null)
      {
        Dialogs.ShowMessage("Требуется заполнить тип документа", MessageType.Error);
        return;
      }
      
      var typeGuid = Guid.Empty;
      if (!Guid.TryParse(propertyStateSetting.EntityTypeGuid, out typeGuid))
      {
        Dialogs.ShowMessage("Указан некорректный гуид типа", MessageType.Error);
        return;
      }
      
      var propertiesDictionary = new Dictionary<string, Sungero.Metadata.PropertyMetadata>();
      var metadata = Sungero.Metadata.Services.MetadataSearcher.FindEntityMetadata(typeGuid);
      
      var selectedNames = StateSettings.As(e.RootEntity).PropertySettings.Select(s => s.PropertyNameGuid);
      foreach (var prop in metadata.Properties.ToList().Where(s => !selectedNames.Contains(s.NameGuid.ToString())))
        propertiesDictionary.Add($"{prop.GetLocalizedName()} ({prop.Name})", prop);
      
      var dialog = Dialogs.CreateInputDialog("Выбор свойства");
      var property = dialog.AddSelect("Свойство", true, 0).From(propertiesDictionary.Select(s => s.Key).ToArray());
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var item = propertiesDictionary.ElementAt(property.ValueIndex).Value;
        var row = propertyStateSetting.PropertySettings.AddNew();
        row.LocalisedName = item.GetLocalizedName();
        row.PropertyName = item.Name;
        row.PropertyNameGuid = item.NameGuid.ToString();
      }
    }
  }

}