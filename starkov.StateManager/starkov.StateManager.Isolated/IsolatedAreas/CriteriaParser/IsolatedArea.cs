using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using starkov.StateManager.Structures.Module;

namespace starkov.StateManager.Isolated.CriteriaParser
{
  public static class CriteriaParser
  {
    public static ICriteriaExpression ParseJsonToDto(string json)
    {
      try
      {
        if (string.IsNullOrEmpty(json))
          return CriteriaExpression.Create();

        var dto = JsonConvert.DeserializeObject<CriteriaDto>(json);
        var result = CriteriaExpression.Create();
        result.conditions = new List<ICondition>();

        if (dto?.Conditions != null)
        {
          foreach (var conditionDto in dto.Conditions)
          {
            var condition = Condition.Create();
            condition.Id = conditionDto.Id;
            condition.OperatorValue = conditionDto.OperatorValue;
            condition.RightValue = conditionDto.RightValue;
            condition.RightType = conditionDto.RightType;
            condition.RightPropertyType = conditionDto.RightPropertyType;
            condition.RightPropertyGuid = conditionDto.RightPropertyGuid;
            condition.LogicalOperator = conditionDto.LogicalOperator;
            condition.LeftProperty = MapExpression(conditionDto.LeftProperty);
            condition.RightExpression = MapExpression(conditionDto.RightExpression);
            result.conditions.Add(condition);
          }
        }
        return result;
      }
      catch (Exception ex)
      {
        Logger.Error("ParseJsonToDto. ошибка при парсинге критериев состояний свойств", ex);
        return CriteriaExpression.Create();
      }
    }
    
    private static IExpression MapExpression(ExpressionDto dto)
    {
      if (dto == null) 
        return null;

      var expr = Expression.Create();
      expr.Id = dto.Id;
      expr.CalculateFrom = dto.CalculateFrom;
      expr.SelectedProperties = dto.SelectedProperties ?? new List<string>();
      expr.PropertyChain = new List<IPropertyChainItem>();

      if (dto.PropertyChain != null)
      {
        foreach (var p in dto.PropertyChain)
        {
          var item = PropertyChainItem.Create();
          item.ParentGuid = p.ParentGuid;
          item.PropertyGuid = p.PropertyGuid;
          item.PropertyNameGuid = p.PropertyNameGuid;
          item.Name = p.Name;
          item.LocalisedName = p.LocalisedName;
          item.PropertyType = p.PropertyType;
          item.CalculateFrom = p.CalculateFrom;
          item.CalledFromEntityType = p.CalledFromEntityType;
          expr.PropertyChain.Add(item);
        }
      }

      return expr;
    }
    
    private class CriteriaDto
    {
      [JsonProperty("conditions")]
      public List<ConditionDto> Conditions { get; set; }
    }

    private class ConditionDto
    {
      [JsonProperty("id")]
      public string Id { get; set; }
      [JsonProperty("operator")]
      public string OperatorValue { get; set; }
      [JsonProperty("rightValue")]
      public string RightValue { get; set; }
      [JsonProperty("rightType")]
      public string RightType { get; set; }
      [JsonProperty("rightPropertyType")]
      public string RightPropertyType { get; set; }
      [JsonProperty("rightPropertyGuid")]
      public string RightPropertyGuid { get; set; }
      [JsonProperty("logicalOperator")]
      public string LogicalOperator { get; set; }
      [JsonProperty("leftProperty")]
      public ExpressionDto LeftProperty { get; set; }
      [JsonProperty("rightExpression")]
      public ExpressionDto RightExpression { get; set; }
    }

    private class ExpressionDto
    {
      [JsonProperty("Id")]
      public string Id { get; set; }
      [JsonProperty("CalculateFrom")]
      public string CalculateFrom { get; set; }
      [JsonProperty("SelectedProperties")]
      public List<string> SelectedProperties { get; set; }
      [JsonProperty("PropertyChain")]
      public List<PropertyChainItemDto> PropertyChain { get; set; }
    }

    private class PropertyChainItemDto
    {
      [JsonProperty("ParentGuid")]
      public string ParentGuid { get; set; }
      [JsonProperty("PropertyGuid")]
      public string PropertyGuid { get; set; }
      [JsonProperty("PropertyNameGuid")]
      public string PropertyNameGuid { get; set; }
      [JsonProperty("Name")]
      public string Name { get; set; }
      [JsonProperty("LocalisedName")]
      public string LocalisedName { get; set; }
      [JsonProperty("PropertyType")]
      public string PropertyType { get; set; }
      [JsonProperty("CalculateFrom")]
      public string CalculateFrom { get; set; }
      [JsonProperty("CalledFromEntityType")]
      public string CalledFromEntityType { get; set; }
    }
  }
}