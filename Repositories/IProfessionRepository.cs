using FantasyChas_Backend.Data;
using FantasyChas_Backend.Models;
using System.Reflection.Metadata.Ecma335;

namespace FantasyChas_Backend.Repositories
{
    public interface IProfessionRepository
    {
        public Profession GetProfessionById(int professionId);
        public Task AddProfessionAsync();
        public bool CheckIfProfessionsExist();
    }

    public class ProfessionRepository : IProfessionRepository
    {
        private static ApplicationDbContext _context;

        public ProfessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddProfessionAsync()
        {
            Profession profession = new Profession()
            { 
                ProfessionName = "IT-tekniker"
            };

            await _context.Professions.AddAsync(profession);
            await _context.SaveChangesAsync();
        }

        public bool CheckIfProfessionsExist()
        {
            return _context.Professions.Any() ? true : false;
        }

        public Profession GetProfessionById(int professionId)
        {
            try
            {
                Profession? profession = _context.Professions
                    .Where(p => p.Id == professionId)
                    .SingleOrDefault();

                if(profession == null)
                {
                    throw new Exception("No profession found");
                }

                return profession;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
