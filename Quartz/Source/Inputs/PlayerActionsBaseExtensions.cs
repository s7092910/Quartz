using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Inputs
{
    public static class PlayerActionsBaseExtensions
    {
        public static void AddBindingConflictWithActionSet(this PlayerActionsBase self, PlayerActionsBase other)
        {
            PlayerActionData.ActionSetUserData data = self.UserData as PlayerActionData.ActionSetUserData;
            if(data == null)
            {
                self.UserData = new PlayerActionData.ActionSetUserData( new PlayerActionsBase[]
                {
                    other
                });

                return;
            }

            List<PlayerActionsBase> list = new List<PlayerActionsBase>(data.bindingsConflictWithSet);
            if (!list.Contains(other))
            {
                list.Add(other);
                self.UserData = new PlayerActionData.ActionSetUserData(list.ToArray());
            }
        }
    }

}
