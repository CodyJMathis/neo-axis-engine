﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace NeoAxis
{
	/// <summary>
	/// Task component for artificial intelligence.
	/// </summary>
	public class AITask : Component
	{
		double timeCreated;

		/// <summary>
		/// Whether to delete the task when task is successfully completed.
		/// </summary>
		[Category( "AI Task" )]
		[DefaultValue( true )]
		public Reference<bool> DeleteTaskWhenReach
		{
			get { if( _deleteTaskWhenReach.BeginGet() ) DeleteTaskWhenReach = _deleteTaskWhenReach.Get( this ); return _deleteTaskWhenReach.value; }
			set { if( _deleteTaskWhenReach.BeginSet( this, ref value ) ) { try { DeleteTaskWhenReachChanged?.Invoke( this ); } finally { _deleteTaskWhenReach.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="DeleteTaskWhenReach"/> property value changes.</summary>
		public event Action<AITask> DeleteTaskWhenReachChanged;
		ReferenceField<bool> _deleteTaskWhenReach = true;

		/////////////////////////////////////////

		public AITask()
		{
			ResetTimeCreated();
		}

		protected virtual void OnPerformTaskSimulationStep() { }

		public delegate void TaskSimulationStepDelegate( AITask sender );
		public event TaskSimulationStepDelegate TaskSimulationStep;

		public void PerformTaskSimulationStep()
		{
			OnPerformTaskSimulationStep();
			TaskSimulationStep?.Invoke( this );
		}

		[Browsable( false )]
		public double TimeCreated
		{
			get
			{
				if( EngineApp.Instance == null )
					return 0;
				return EngineApp.EngineTime - timeCreated;
			}
		}

		public void ResetTimeCreated()
		{
			if( EngineApp.Instance != null )
				timeCreated = EngineApp.EngineTime;
		}
	}
}