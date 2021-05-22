using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Extensions;
using RandomizerMod.Randomization.Logic;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Randomization.Util
{
    public class PrePlacedManager
    {
        List<ILP> checkList;
        List<ILP> tempList;
        ProgressionManager pm;


        public PrePlacedManager(List<ILP> checkList, ProgressionManager pm)
        {
            this.pm = pm;
            this.checkList = checkList;
            this.tempList = new List<ILP>();
            pm.AfterAddItem += Check;
            pm.AfterEndTemp += EndTemp;
            Check(false);
        }

        public void Check(bool temp)
        {
            for (int i = 0; i < checkList.Count; i++)
            {
                ILP pair = checkList[i];
                if (pm.CanGet(pair.location))
                {
                    checkList.RemoveAt(i);
                    if (temp) tempList.Add(pair);
                    pm.Add(pair.item);
                    break; // pm hook triggers new search
                }
            }
        }

        public void EndTemp(bool saveTemp)
        {
            if (saveTemp) tempList.Clear();
            else while (tempList.Any())
                {
                    ILP pair = tempList.Pop();
                    checkList.Add(pair);
                }
        }

    }
}
