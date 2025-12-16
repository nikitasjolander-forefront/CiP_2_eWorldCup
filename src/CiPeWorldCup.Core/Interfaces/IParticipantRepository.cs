using CiPeWorldCup.Core.Entities;

namespace CiPeWorldCup.Core.Interfaces;
public interface IParticipantRepository
{
    Task<IEnumerable<Participant>> GetAllParticipantsAsync();
    Task<Participant?> GetParticipantByIdAsync(int id);
    Task<Participant> AddParticipantAsync(Participant participant);
    Task<Participant> RemoveParticipantByIdAsync(int id);

}
