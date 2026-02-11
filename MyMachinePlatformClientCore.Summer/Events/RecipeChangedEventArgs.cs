using MyMachinePlatformClientCore.Summer.Common;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Events
{
    public class RecipeChangedEventArgs : EventArgs
    {
        public Recipe NewRecipe { get; }

        public Recipe OldRecipe { get; }

        public RecipeChangedEventArgs(Recipe newRecipe, Recipe oldRecipe)
        {
            this.NewRecipe = newRecipe;
            this.OldRecipe = oldRecipe;
        }
    }
}
