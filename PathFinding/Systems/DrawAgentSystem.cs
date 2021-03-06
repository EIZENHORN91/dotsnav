using DotsNav.Drawing;
using DotsNav.PathFinding.Data;
using DotsNav.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DotsNav.PathFinding
{
    [UpdateInGroup(typeof(DotsNavDrawingSystemGroup))]
    class DrawAgentSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithBurst()
                .ForEach((AgentComponent agent, AgentDrawComponent debug, DynamicBuffer<PathSegmentElement> path) =>
                {
                    if (!debug.Draw || path.Length == 0)
                        return;

                    var lines = new NativeList<Line>(Allocator.Temp);
                    var color = debug.Color;
                    if (color.a < 10)
                        color.a += 10;

                    for (int j = 0; j < path.Length; j++)
                    {
                        var segment = path[j];
                        var perp = math.normalize(Math.PerpCcw(segment.To - segment.From)) * agent.Radius;
                        lines.Add(new Line(segment.From + perp, segment.To + perp, color));
                        lines.Add(new Line(segment.From - perp, segment.To - perp, color));
                    }

                    var up = new float3(0, 1, 0);

                    for (int j = 1; j < path.Length; j++)
                    {
                        var from = path[j - 1].To;
                        var c = path[j].Corner;
                        var to = path[j].From;
                        var angle = (Angle) Math.Angle(to - c) - Math.Angle(from - c);
                        Arc.Draw(lines, c.ToXxY(), up, 2 * (from - c).ToXxY(), angle, color, agent.Radius, debug.Delimit);
                    }

                    var arm = new float3(0, 0, agent.Radius);
                    Arc.Draw(lines, path[0].From.ToXxY(), up, arm, 2 * math.PI, color, agent.Radius, debug.Delimit);
                    Arc.Draw(lines, path[path.Length - 1].To.ToXxY(), up, arm, 2 * math.PI, color, agent.Radius, debug.Delimit);

                    Line.Draw(lines);
                })
                .Schedule();

            DotsNavRenderer.Handle = JobHandle.CombineDependencies(DotsNavRenderer.Handle, Dependency);
        }
    }
}