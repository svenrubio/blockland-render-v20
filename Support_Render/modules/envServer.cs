//----------------------------------------------------------------------
// Title:   Server-Sided Environment Utilities
// Author:  Lake_YS
// Version: 1
// Updated: December 19, 2016
//----------------------------------------------------------------------

if($Library::EnvServer::Ver >= 1)
	return;

$Library::EnvServer::Ver = 1;

// Returns 1 if the day/night cycle is enabled.
function env_getDayCycleEnabled()
{
	//// Testing Criteria ////
	// Ensure this returns 1 if:
	// Day cycle environment selected in simple mode
	// Day cycle environment selected, day cycle enabled in advanced.
	// Non-day cycle environment selected, day cycle enabled in advanced.
	//
	// Ensure this does NOT return 1 if:
	// Non-day cycle env is selected, day cycle is enabled in advanced, environment is set to simple mode

	return ($Sky::DayCycleEnabled && $EnvGuiServer::SimpleMode) || ($EnvGuiServer::DayCycleEnabled && !$EnvGuiServer::SimpleMode);
}
