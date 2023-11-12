# Woolich Log File Decoder

**Warning: This repo and it's owner has no association nor affiliation with Woolich Racing. This repo came into existence in order to overcome limitations and bugs in the woolich software and to enable users of the woolich logging tools to access their own data**

### What does this do?
This application decodes the log files collected by the woolich Log Box-D CAN v3 logger and allows for 2 main types of output...
* CSV output of the log file contents
* A filtered file suitable for use with autotune to correct errors in autotune's own filtering.

Additional outputs exist for use in decoding the log file.

### License
While this is GNU GPLv2 please respect Woolich's IP and don't attempt to undercut their business using this software. Honestly I don't know how you could with this, because this actually makes their software better, but it needs to be said.



### Autotune Filtering functionality
The autotune filter relies on setting all filter conditions to gear 0 (neutral) so that the existing autotune gear 0 filter will exclude them without disrupting any valid AFR records that may exist in that record. AFR can be delayed up to 600ms.

* remove logs for gear 2
* Remove gear 1 logs below idle RPM (1000RPM) and above 4500 RPM so that we're only handling GEAR 1 AFR for launches.
* Remove idling records where the RPM below 1200 RPM
* Remove closed throttle engine braking for gears 1, 2 and 3
* Compensate for the MT09 ETV calculation error in the woolich software that puts low and high ETV AFR values in the wrong cells.


### Supported ECU's 
Currently supported:
- MT09SP 2023

Potentially supported:
- MT09SP 2021 - 2022
- MT09 2021 - 2023

If you have logs for any other type of bike feel free to send them in and I will try and decode them. While I won't be able to directly compare them to the values in the woolich software (I would need a key for the bin file) I can decode it and send it back for you to compare.


### Why does this repo exist?

The reason for this can be understood by watching the youtube playlist 
https://www.youtube.com/watch?v=mNZLtrzuuwY&list=PLKNjpzEkK2wLMr8a8zEwzBNT63SJbxuuI
where I work my way through the many aspects (both good and bad) of the Woolich software.

There are many limitations in the Woolich Racing software and they "presumably" don't have the resources to fix every little issue pertainint to one single model of motorcycle out of the somewhat crazy number of bikes that are out there. Thats a realistic approach from their perspective and there's no negativity directed towards them. We're all just grateful that they're decoding the ECU's at all and allowing us to have our bikes flashed.

### Disclaimer
The repo is obviously a simple winforms application in c#. That's because I needed something simple that could be put together easily and modified easily as new functions were needed. This repo is not intended to showcase my development skills. I have a day job for that.

TODO:
Implement some CI/CD mechanisms and an installer bundle once I work out how. It's my first time with public repos :-)

