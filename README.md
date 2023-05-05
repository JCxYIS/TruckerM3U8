# TruckerM3U8
Convert (not only) m3u8 stream to mp3 that ETS2/ATS can recognize.

## How to use

Download `TruckerM3U8_x64.zip` from [the release page](https://github.com/JCxYIS/TruckerM3U8/releases/latest)

Unzip and execute `TruckerM3U8.exe`, a browser screen will show up

![](https://i.imgur.com/Zbyd0I2.png)

Pick a radio and play, you can preview the audio from the above link (http://localhost:3378/).

You can add your own radio in `wwwroot/radio.json`

![](https://i.imgur.com/UHCEv7z.png)


## How does it work
lol it's just ffmpeg launcher
```
+-------------+                       
|   FFMPEG    |  (Download & convert m3u8 to mp3 stream)                   
+-------------+ 
     |
     | tcp (port 1049)
     |
     v
+-------------+
| TruckerM3U8 | (Distribute mp3 stream)
+-------------+
     |
     | tcp (port 3378)
     |
     v
+-------------+
| User (ETS2) |
+-------------+
```

## My Truck

🥰🥰🥰

![](https://i.imgur.com/cUkSVLW.jpg)
