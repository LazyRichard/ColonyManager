﻿// // Karel Kroeze
// // Trigger_PawnKind.cs
// // 2016-12-09

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class Trigger_PawnKind : Trigger
    {
        #region Constructors

        public Trigger_PawnKind( Manager manager ) : base( manager ) { CountTargets = Utilities_Livestock.AgeSexArray.ToDictionary( k => k, v => 5 ); }

        #endregion Constructors

        #region Fields

        public Dictionary<Utilities_Livestock.AgeAndSex, int> CountTargets;
        public PawnKindDef pawnKind;

        private Utilities.CachedValue<bool> _state = new Utilities.CachedValue<bool>();

        #endregion Fields

        #region Properties

        public int[] Counts
        {
            get
            {
                return Utilities_Livestock.AgeSexArray.Select( ageSex => pawnKind.GetTame( manager, ageSex ).Count ).ToArray();
            }
        }

        public ManagerJob_Livestock Job
        {
            get
            {
                return manager.JobStack.FullStack<ManagerJob_Livestock>()
                              .FirstOrDefault( job => job.Trigger == this );
            }
        }

        public override bool State
        {
            get
            {
                bool state;
                if ( !_state.TryGetValue( out state ) )
                {
                    state =
                        Utilities_Livestock.AgeSexArray.All(
                                                            ageSex =>
                                                            CountTargets[ageSex] == pawnKind.GetTame( manager, ageSex ).Count ) &&
                        AllTrainingWantedSet();
                    _state.Update( state );
                }
                return state;
            }
        }

        public override string StatusTooltip
        {
            get
            {
                var tooltipArgs = new List<string>();
                tooltipArgs.Add( pawnKind.LabelCap );
                tooltipArgs.AddRange( Counts.Select( x => x.ToString() ) );
                tooltipArgs.AddRange( CountTargets.Values.Select( v => v.ToString() ) );
                return "FML.ListEntryTooltip".Translate( tooltipArgs.ToArray() );
            }
        }

        #endregion Properties

        #region Methods

        public override void DrawTriggerConfig( ref Vector2 cur, float width, float entryHeight, bool alt = false,
                                                string label = null, string tooltip = null )
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.LookDictionary( ref CountTargets, "Targets", LookMode.Value, LookMode.Value );
            Scribe_Defs.LookDef( ref pawnKind, "PawnKind" );
        }

        private bool AllTrainingWantedSet()
        {
            // do a dry run of the training assignment (no assignments are set).
            // this is rediculously expensive, and should never be called on tick.
            var actionTaken = false;
            Job.DoTrainingJobs( ref actionTaken, false );
            return actionTaken;
        }

        #endregion Methods
    }
}