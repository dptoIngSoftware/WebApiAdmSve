using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface ICandidato_Repository
    {
        List<Candidato> GetAllByVotacionID(int ID);
        List<Candidato> GetAllSelecByVotacionID(int ID);
        Candidato GetById(int ID);
        Candidato GetByIdVotacionGanador(int IDVE);
        List<Candidato> GetAllSelected(int IDVE);
        bool CreateAll(List<Candidato> Candidatos);
        bool UpdateAll(List<Candidato> Candidatos);
        bool Update(Candidato Candidato);
        bool SaveAll(int Length);
        bool Save();
    }
}
