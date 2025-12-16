using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.Interfaces;

namespace CiPeWorldCup.Application.Services;
public class ParticipantService
{
    private readonly IParticipantRepository _participantRepository;

    public ParticipantService(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository;
    }
    
    public async Task<IEnumerable<Participant>> GetAllParticipantsAsync()
    {
        return await _participantRepository.GetAllParticipantsAsync();
    }

    public async Task<Participant?> GetParticipantByIdAsync(int id)
    {
        return await _participantRepository.GetParticipantByIdAsync(id);
    }

    public async Task<Participant> AddParticipantAsync(Participant participant)
    {
        return await _participantRepository.AddParticipantAsync(participant);
    }

    public async Task<Participant> RemoveParticipantByIdAsync(int id)
    {
        return await _participantRepository.RemoveParticipantByIdAsync(id);
    }
}
