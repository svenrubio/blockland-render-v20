// experimental timeSupport

function getDayCycleTime() //Returns the time in seconds from dawn from the current time
{
	%curTime = $Sim::Time; // the day cycle is tied directly to the simTime of the server.

	%length = DayCycle.dayLength; // this is how long in seconds the day will last. for example, 24 is a full 24 hour day in the space of 24 seconds.

	%offset = DayCycle.dayOffset; // this is how much offset is applied to the day.

	%offset = mfloor(%offset * %length); // this calculates how many seconds the offset actually affects, as the offset is defined in a range between 0 and 1.

	if(%offset != 0)
		%curTime = (%curTime + %offset) - %length; // this adds the offset into the equation.

	%final = mFloor(%curTime % %length); // this determines the remainder of simTime divided by length, which is how far, in seconds, we are into the day.

	if(%final < 0) //if the result is a negative, we need to subtract it from the total. this allows compatibility with offsets.
		%final = %length - %final;

	return %final;
}

function getPartOfDayCycle() //Returns 3, 2, 1, 0 for Midnight, Dusk, Noon, Dawn respectively
{
	%tod = getDayCycleTime(); // determine time into the day.

	%len = DayCycle.dayLength; // determine length of days

	%quarter = mFloatLength(%len / 4, 0);

	%part = mFloor(%tod / %quarter); // this ought to return a number like 1 2 3 4

	return %part;
}

function getNextDayCycleTime() // This is used to intelligently determine the schedule wait time.
{
	%tod = getDayCycleTime(); // establish a starting point

	%len = dayCycle.dayLength; // establish length of days

	//thanks to Zack0Wack0 for this
	%quarter = mFloatLength(%len / 4, 0);

	DayCyclesDebug("Quarter: " @ %quarter);

	%part = mFloor(%tod / %quarter);

	DayCyclesDebug("Part of Day: " @ %part);

	%future = (%part + 1) * %quarter - %tod;

	DayCyclesDebug("The future occurs in: " @ %future @ " seconds.");

	%future *= 1000;

	return %future;
}
