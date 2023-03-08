module Utils

open Brahma.FSharp
open Expecto

let isDoubleCompatible (device: ClDevice) = Array.contains CL_KHR_FP64 device.DeviceExtensions

let isDeviceCompatibleTest<'a> device =
    let isFloat64test = typeof<'a>.IsEquivalentTo typeof<float>

    let isDeviceFloat64Compatible = isDoubleCompatible device

    match isDeviceFloat64Compatible, isFloat64test with
    | true, true
    | false, false -> true
    | _ -> false

let float32IsEqual (x: float32) (y: float32) = float (abs (x - y)) < Accuracy.low.relative

let floatIsEqual (x: float) (y: float) = abs (x - y) < Accuracy.low.relative
