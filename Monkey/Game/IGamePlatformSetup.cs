using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;

namespace Monkey.Game
{
    public interface IGamePlatformSetup
    {
        void RegisterBundles(BundleCollection bundles);

        void LoadGameData();
    }
}
