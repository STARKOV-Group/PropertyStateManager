using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using starkov.PropertyStateManager.OfficialDocument;
using Sungero.Domain.Shared;
using System.Linq.Expressions;
using System.Collections.Concurrent;

using StateKinds = starkov.StateManager.PublicConstants.Module.StateKinds;

namespace starkov.PropertyStateManager.Shared
{
  partial class OfficialDocumentFunctions
  {
    private static ConcurrentDictionary<string, Func<object, object>> _gettersCache = new ConcurrentDictionary<string, Func<object, object>>();
    
    /// <summary>
    /// Установить состояния свойств.
    /// </summary>
    [Public]
    public virtual void SetPropertiesStates()
    {
      var stateSetting = GetStateSettingForDocument();
      if (stateSetting == null)
        return;
      
      var propState = _obj.State.Properties;
      var propStateDict = propState.ToDictionary(
        p => p.GetType().GetProperty("PropertyName")?.GetValue(p)?.ToString(),
        p => p
       );
      
      foreach (var setting in stateSetting.PropertySettings)
      {
        Sungero.Domain.Shared.IPropertyStateBase property = null;
        
        if (propStateDict.TryGetValue(setting.PropertyName, out property))
        {
          property.IsEnabled = CalculateEnabled(setting, property.IsEnabled);
          property.IsVisible = CalculateVisability(setting, property.IsVisible);
          property.IsRequired = CalculateRequired(setting, property.IsRequired);
        }
      }
    }
    
    /// <summary>
    /// Получить настройки состояния свойств.
    /// </summary>
    /// <returns>Настройки состояния свойств.</returns>
    [Public]
    public virtual StateManager.IStateSetting GetStateSettingForDocument()
    {
      var documentTypeGuid = _obj.DocumentKind?.DocumentType?.DocumentTypeGuid;
      if (string.IsNullOrEmpty(documentTypeGuid))
      {
        var docType = _obj.GetType();
        
        var typeProperty = docType.GetProperty("TypeGuid");
        if (typeProperty != null)
          documentTypeGuid = ((Guid)typeProperty.GetValue(_obj)).GetOriginalTypeGuid().ToString();
      }
      
      if (string.IsNullOrEmpty(documentTypeGuid))
        return StateManager.StateSettings.Null;
      
      return StateManager.StateSettings.GetAll()
        .Where(s => s.Status == StateManager.StateSetting.Status.Active && s.DocType.DocumentTypeGuid == documentTypeGuid)
        .OrderByDescending(s => s.Priority)
        .FirstOrDefault();
    }
    
    #region Вспомогательные методы расчета состояния свойств.
    
    private bool CalculateVisability(starkov.StateManager.IStateSettingPropertySettings setting, bool baseState)
    {
      var isVisible = setting.Visible == StateManager.StateSettingPropertySettings.Visible.Yes ||
        setting.Visible == StateManager.StateSettingPropertySettings.Visible.BaseLogic && baseState;
      
      if (isVisible && !string.IsNullOrEmpty(setting.VisabilityCriteria))
      {
        var conditionExpression = LoadConditionExpression(setting.VisabilityCriteria, setting.PropertyName, StateKinds.IsVisible);
        isVisible = StateManager.PublicFunctions.Module.CalculateConditions(conditionExpression, _obj);
      }
      
      return isVisible;
    }
    
    private bool CalculateEnabled(starkov.StateManager.IStateSettingPropertySettings setting, bool baseState)
    {
      var isEnable = setting.Enabled == StateManager.StateSettingPropertySettings.Enabled.Yes ||
        setting.Enabled == StateManager.StateSettingPropertySettings.Enabled.BaseLogic && baseState;
      
      if (isEnable && !string.IsNullOrEmpty(setting.EnabledCriteria))
      {
        var conditionExpression = LoadConditionExpression(setting.EnabledCriteria, setting.PropertyName, StateKinds.IsEnabled);
        isEnable = StateManager.PublicFunctions.Module.CalculateConditions(conditionExpression, _obj);
      }
      
      return isEnable;
    }
    
    private bool CalculateRequired(starkov.StateManager.IStateSettingPropertySettings setting, bool baseState)
    {
      var isRequired = setting.Required == StateManager.StateSettingPropertySettings.Required.Yes ||
        setting.Required == StateManager.StateSettingPropertySettings.Required.BaseLogic && baseState;
      
      if (isRequired && !string.IsNullOrEmpty(setting.RequiredCriteria))
      {
        var conditionExpression = LoadConditionExpression(setting.RequiredCriteria, setting.PropertyName, StateKinds.IsRequired);
        isRequired = StateManager.PublicFunctions.Module.CalculateConditions(conditionExpression, _obj);
      }
      
      return isRequired;
    }
    
    private StateManager.Structures.Module.ICriteriaExpression LoadConditionExpression(string jsonExpression, string paramName, string stateKind)
    {
      var conditionExpression = StateManager.Structures.Module.CriteriaExpression.Create();
      
      object result = null;
      ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params.TryGetValue($"{paramName}_{stateKind}", out result);
      if (result != null)
      {
        conditionExpression = result as StateManager.Structures.Module.ICriteriaExpression;
      }
      else
      {
        conditionExpression = StateManager.IsolatedFunctions.CriteriaParser.LoadCriteriaFromJson(jsonExpression);
        ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params[$"{paramName}_{stateKind}"] = conditionExpression;
      }
      
      return conditionExpression;
    }
    
    #endregion
    
    #region Получение значения свойства выражения для документа.
    
    [Public]
    public static object GetPropertyValueFromOfficialDocument(IEntity entity, StateManager.Structures.Module.IExpression leftProperty)
    {
      if (entity == null || leftProperty == null || leftProperty.PropertyChain == null || !leftProperty.PropertyChain.Any())
        return null;
      
      object currentEntity = entity;
      foreach (var chainItem in leftProperty.PropertyChain)
      {
        if (currentEntity == null)
          return null;
        
        currentEntity = GetEntityPropertyValue(currentEntity, chainItem);
      }
      
      return currentEntity;
    }
    
    private static object GetEntityPropertyValue(object entity, StateManager.Structures.Module.IPropertyChainItem propertyChain)
    {
      if (entity == null && propertyChain == null)
        return null;
      
      var objType = entity.GetType().GetFinalType();
      var propertyName = propertyChain.Name;
      var key = $"{objType.FullName}.{propertyName}";

      if (!_gettersCache.TryGetValue(key, out var getter))
      {
        getter = BuildPropertyGetter(objType, propertyName);
        getter = _gettersCache.GetOrAdd(key, getter);
      }

      return getter(entity);
    }

    private static Func<object, object> BuildPropertyGetter(Type objType, string propertyName)
    {
      var param = Expression.Parameter(typeof(object), "entity");

      if (TryBuildGetter(param, objType, propertyName, out var getter))
        return getter;

      foreach (var iface in objType.GetInterfaces())
      {
        if (TryBuildGetter(param, iface, propertyName, out getter))
          return getter;
      }

      Logger.Debug($"Свойство '{propertyName}' не найдено в {objType.FullName}");
      return _ => null;
    }

    private static bool TryBuildGetter(ParameterExpression param, Type targetType, string propertyName, out Func<object, object> getter)
    {
      getter = null;
      try
      {
        var converted = Expression.Convert(param, targetType);
        var property = Expression.Property(converted, propertyName);
        var boxed = Expression.Convert(property, typeof(object));
        getter = Expression.Lambda<Func<object, object>>(boxed, param).Compile();
        return true;
      }
      catch (Exception ex)
      {
        Logger.Error(ex, $"TryBuildGetter: не удалось построить getter для {targetType.Name}.{propertyName}:");
        return false;
      }
    }
    
    #endregion
  }
}