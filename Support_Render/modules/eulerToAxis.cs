// Sources:
// https://forum.blockland.us/index.php?topic=27230
// https://forum.blockland.us/index.php?topic=298443

function eulerToAxis(%euler)
{
	%euler = VectorScale(%euler,$pi / 180);
	%matrix = MatrixCreateFromEuler(%euler);
	return getWords(%matrix,3,6);
}

function axisToEuler(%axis)
{
	%angleOver2 = getWord(%axis,3) * 0.5;
	%angleOver2 = -%angleOver2;
	%sinThetaOver2 = mSin(%angleOver2);
	%cosThetaOver2 = mCos(%angleOver2);
	%q0 = %cosThetaOver2;
	%q1 = getWord(%axis,0) * %sinThetaOver2;
	%q2 = getWord(%axis,1) * %sinThetaOver2;
	%q3 = getWord(%axis,2) * %sinThetaOver2;
	%q0q0 = %q0 * %q0;
	%q1q2 = %q1 * %q2;
	%q0q3 = %q0 * %q3;
	%q1q3 = %q1 * %q3;
	%q0q2 = %q0 * %q2;
	%q2q2 = %q2 * %q2;
	%q2q3 = %q2 * %q3;
	%q0q1 = %q0 * %q1;
	%q3q3 = %q3 * %q3;
	%m13 = 2.0 * (%q1q3 - %q0q2);
	%m21 = 2.0 * (%q1q2 - %q0q3);
	%m22 = 2.0 * %q0q0 - 1.0 + 2.0 * %q2q2;
	%m23 = 2.0 * (%q2q3 + %q0q1);
	%m33 = 2.0 * %q0q0 - 1.0 + 2.0 * %q3q3;
	return mRadToDeg(mAsin(%m23)) SPC mRadToDeg(mAtan(-%m13, %m33)) SPC mRadToDeg(mAtan(-%m21, %m22));
}

function rotatePlayerRelative(%player,%val)
{
   if(-360 > %val > 360)
      return;

   if(%val > 179)
   {
      %val = %val / 2;
      %count = 2;
   }
   else %count = 1;

   for(%i=0; %i<%count; %i++)
   {
      %pos = getWords(%player.getTransform(),0,2);
      %euler = axisToEuler(getWords(%player.getTransform(),3,6));
      %rot = getWords(%euler,0,1);
      %newAngleTwo = getWord(%euler,2) + %val;
      if(%newAngle < 0)
         %newAngleTwo = (%newAngle)-%val;
      %newRot = %rot SPC %newAngleTwo;
      %player.setTransform(%pos SPC eulerToAxis(%newRot));
   }
}

