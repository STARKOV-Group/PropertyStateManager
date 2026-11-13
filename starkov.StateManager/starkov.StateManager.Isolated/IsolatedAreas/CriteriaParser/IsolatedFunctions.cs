using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using starkov.StateManager.Structures.Module;

namespace starkov.StateManager.Isolated.CriteriaParser
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Заполнить структуру с критериями из JSON.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    [Public]
    public static StateManager.Structures.Module.ICriteriaExpression LoadCriteriaFromJson(string json)
    {
      return CriteriaParser.ParseJsonToDto(json);
    }
  }
}