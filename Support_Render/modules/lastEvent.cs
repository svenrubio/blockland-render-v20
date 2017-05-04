//------------------------------------------------------------------------------
// Title:   Client - Last Event
// Author:  Lake_YS
// Version: 1
// Updated: May 4, 2017
//------------------------------------------------------------------------------

if($Library::LastEvent::Ver >= 1)
	return;

$Library::LastEvent::Ver = 1;

deactivatePackage("Support_LastEvent");
package Support_LastEvent
{
  function SimObject::processInputEvent(%this, %eventName, %client)
  {
    Parent::processInputEvent(%this, %eventName, %client);
    %client.lastEventBrick = %this;
  }
};
activatePackage("Support_LastEvent");
