# TVShow
Watch movies easier

![screenshot](https://cloud.githubusercontent.com/assets/8962802/7101211/1a3746ae-e04d-11e4-8ed7-b2d0bd5bc221.jpg)

## What is TVShow?
TVShow is a project which aims to provide a simple interface to watch any movies.

## Well, is it legal?
Depending on which country you live in, it may be not. Also, I'm planning to rethink the media's content to make it entirely legal (ie. content may be replaced by free instructional media such as documentaries or TED podcasts).

## Hmm, it does remember me something...
Well, you may be thinking about Popcorn Time, don't you? If so, yes. The concept is not new, and this project is clearly inspired from it. Also, my primary goal was to make an alternative, using .NET framework.

## What does it use?
I use .NET Framework 4.5 and C# for the backend. WPF/XAML for the interface. Libtorrent-rasterbar for torrent downloading.

## Supported platforms
At this time, only Windows 7+ is supported (Windows 7, 8, 8.1, 10 | 32/64 Bits). I plan to make it cross-platform using Mono.

## Can I help you?
Of course yes! It's open-source so feel free to submit pull-request, I promise I'll consider them.

## Visual Studio .sln
The solution is built using Visual Studio 2013.

## TVShow VS Popcorn Time ?
Well, there's some points.

* Cross-platform. Popcorn Time is based on Node-WebKit, hence, on NodeJS. Consequently, it's cross-platform. At the opposite, TVShow is fully built using .NET/C#. So, multi-platform does not come out of the box. However, Mono works well and is aimed to support multi-platform, so that future support for multiple OS is conceivable.

* Performances. Since NodeJS is based on V8 VM, it is fast. Really fast. But compared to a fully native app, it's a slow rabbit. That's why Popcorn Time compares bad to TVShow, whose performances are great.

* Features. Since TVShow is a recent project, Popcorn Time supports more features that TVShow does. But it's just a matter of time.

* Stability. TVShow appears to be more stable than Popcorn Time. Sometimes, Popcorn Time crashes randomly. Not sayin I guarantee it will never happen to TVShow, but unit tests are there. Seems not to be considered in Popcorn Time projet...

* API. It's simple: TVShow uses the YTS and The Movie Database APIs, the same that Popcorn Time uses.
