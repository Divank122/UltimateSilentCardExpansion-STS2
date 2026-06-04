using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace USCE.Scripts;

/// <summary>
/// 动能卡牌接口。实现此接口的卡牌在每次打出后，其指定的变量会自动+1。
/// </summary>
public interface IKineticCard
{
    /// <summary>
    /// 返回需要每次打出后+1的变量列表。
    /// </summary>
    IEnumerable<DynamicVar> GetKineticVars();
}
