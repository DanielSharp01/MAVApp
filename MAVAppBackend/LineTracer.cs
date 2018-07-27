using MAVAppBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    public class LineTracer
    {
        private Polyline tracable;
        private List<Line> lines = new List<Line>();
        private double distance = 0;

        public bool IsOver = false;

        public LineTracer(Polyline tracable, IEnumerable<Line> lines)
        {
            this.tracable = tracable;
            this.lines.AddRange(lines);
        }

        public Line Trace(Line dontBreak = null)
        {
            List<Line> candidates = new List<Line>(lines);

            Line lastCandidate = null;
            while (candidates.Count > 0 && !IsOver)
            {
                Vector2 segB = tracable.GetPoint(distance);
                Vector2 segE = tracable.GetPoint(distance + 0.5);
                if (segE == tracable.Points.Last())
                    IsOver = true;

                lastCandidate = candidates.Contains(dontBreak) ? dontBreak : candidates.First();
                candidates.RemoveAll(c => double.IsNaN(c.Polyline.GetProjectedDistance(segB, 0.01)) || double.IsNaN(c.Polyline.GetProjectedDistance(segE, 0.01)));

                distance += 0.5;
            }

            if (lastCandidate == null) throw new InvalidOperationException("No candidate line could be found!");
            return lastCandidate;
        }
    }
}
