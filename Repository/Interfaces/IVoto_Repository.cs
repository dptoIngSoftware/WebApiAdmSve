using WebApiVotacionElectronica.Models.DataHolder;
using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface IVoto_Repository
    {
        List<Voto> GetByVotacion(int VotacionID);
        List<int> TopCandidatos(int top ,int VotacionID);
        bool CreateAll(List<Voto> Votos);
        bool SaveAll(int Length);
        int TotalVotosByIDVotacion(int ID_Votacion);
        int TotalNulosByIDVotacion(int ID_Votacion);
        List<CandidatoxVoto_DataHolder> Candidatos(int top, int VotacionID);
        List<CandidatoxVoto_DataHolder> CandidatosDisponibles(int top, int VotacionID);
        List<CandidatoxVoto_DataHolder> CandidatosSeleccionados(int top, int VotacionID);
        List<CandidatoxVoto_DataHolder> CandidatosAceptados(int top, int VotacionID);
        int votosByIDcandidato(int IDC);

        int VotantesqueVotaron(int ID_Votacion);
    }
}
