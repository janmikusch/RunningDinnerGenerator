using System.Collections.Generic;
using System.Linq;

namespace RunningDinner
{
    public class Course
    {
        public CourseType Type { get; set; }
        public Team Cook { get; set; }
        public List<Team> Guests { get; set; } = new List<Team>();

        public bool HasGuest(Team t)
        {
            return Enumerable.Contains(this.Guests, t);
        }
    }
}