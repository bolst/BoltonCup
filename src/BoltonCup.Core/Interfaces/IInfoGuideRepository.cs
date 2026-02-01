using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface IInfoGuideRepository : IRepository<InfoGuide, GetInfoGuidesQuery, Guid>
{
}