using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace USCE.Scripts;

public interface IKineticCard
{
    IEnumerable<DynamicVar> GetKineticVars();
}
