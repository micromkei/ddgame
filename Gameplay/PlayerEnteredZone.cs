using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{ 
    public class PlayerEnteredZone : Simulation.Event<PlayerEnteredZone>
    {
        public TalkZone talkZone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            model.player.animator.SetTrigger("victory");
            model.player.controlEnabled = false;
        }
    }
}