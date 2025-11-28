using CiPeWorldCup.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.Interfaces;
public interface IParticipantRepository
{
    Task<IEnumerable<Participant>> GetAllParticipantsAsync();
    Task<Participant?> GetParticipantByIdAsync(int id);
    Task<Participant> AddParticipantAsync(Participant participant);
    Task<Participant> RemoveParticipantByIdAsync(int id);

}
