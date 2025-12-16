using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.Interfaces;
using CiPeWorldCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CiPeWorldCup.Infrastructure.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly CiPeWorldCupDbContext _context;
    public ParticipantRepository(CiPeWorldCupDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Participant>> GetAllParticipantsAsync()
    {
        return await _context.Participants.ToListAsync();
    }
    public async Task<Participant?> GetParticipantByIdAsync(int id)
    {
        return await _context.Participants.FindAsync(id);
    }
    public async Task<Participant> AddParticipantAsync(Participant participant)
    {
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();
        return participant;
    }
    public async Task<Participant> RemoveParticipantByIdAsync(int id)
    {
        var participant = await _context.Participants.FindAsync(id);
        if (participant != null)
        {
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
        }
        return participant!;
    }

}
