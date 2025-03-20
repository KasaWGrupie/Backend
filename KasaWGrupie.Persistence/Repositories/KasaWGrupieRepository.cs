using Ardalis.Specification.EntityFrameworkCore;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Data;

namespace KasaWGrupie.Persistence.Repositories;

public class KasaWGrupieRepository<T> : RepositoryBase<T> where T : EntityBase
{
	protected readonly KasaWGrupieDbContext _dbContext;
	public KasaWGrupieRepository(KasaWGrupieDbContext dbContext) : base(dbContext)
	{
		_dbContext = dbContext;
	}
}