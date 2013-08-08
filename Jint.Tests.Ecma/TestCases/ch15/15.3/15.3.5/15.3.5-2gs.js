/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.3/15.3.5/15.3.5-2gs.js
 * @description StrictMode - error is thrown when reading the 'caller' property of a function object
 * @onlyStrict
 * @negative ^((?!NotEarlyError).)*$
 */

"use strict";
function _15_3_5_1_gs() {}
_15_3_5_1_gs.caller;
throw NotEarlyError;