// Copyright 2009 the Sputnik authors.  All rights reserved.
/**
 * If x is -Infinity, Math.tan(x) is NaN
 *
 * @path ch15/15.8/15.8.2/15.8.2.18/S15.8.2.18_A5.js
 * @description Checking if Math.tan(-Infinity) is NaN
 */

// CHECK#1
var x = -Infinity;
if (!isNaN(Math.tan(x)))
{
	$ERROR("#1: 'var x=-Infinity; isNaN(Math.tan(x)) === false'");
}

