Mandela
=======

An application to generate fractals and other 2 dimensional set visualizations.  This program is very old.  It was written in 2004, when [Mono](http://www.mono-project.com/) first came out.  My goal for the application was to learn GTK# and play around with it as a potential tool for cross-platform user interface development.  The outcome was that I never used GTK# in any professional sense, as it was ... flakey and inconsistent across environments.  It did, however, work great in Linux and appears to work very well for being 10 years old on my OS/X machine.

## Building the Application

Mandela only builds from the command-line, using the NANT build system.  It is quite possible you could dump the files directly into an IDE and have a magical working application, but you would need to make sure the right libraries are connected.  There appears to be a warning about using an obsolete mechanism for the GTK progress bar.

	steven at MathBookPro in ~/Dropbox/Code/mandela on master*
	$ nant
	NAnt 0.92 (Build 0.92.4543.0; release; 6/9/2012)
	Copyright (C) 2001-2012 Gerry Shaw
	http://nant.sourceforge.net

	Buildfile: file:///Users/steven/Dropbox/Code/mandela/Mandela.build
	Target framework: Mono 2.0 Profile
	Target(s) specified: build

	 [property] Target framework changed to "Mono 4.5 Profile".

	build:

	      [csc] Compiling 18 files to '/Users/steven/Dropbox/Code/mandela/bin/Mandela.exe'.
	      [csc] /Users/steven/Dropbox/Code/mandela/MandelaWindow.cs(44,29): warning CS0612: `Gtk.ProgressBar.BarStyle' is obsolete
	      [csc] Compilation succeeded - 1 warning(s)

	BUILD SUCCEEDED - 0 non-fatal error(s), 1 warning(s)

	Total time: 0.7 seconds.

	steven at MathBookPro in ~/Dropbox/Code/mandela on master*
	$

## Running the Application

A shell script is also copied in the build process (and is new, as of 2014) to allow the legacy GTK widgets to be linked and work properly.  You just run 'bin/Mandela.sh' to start the GUI up.

On the command-line, where you executed the application, you will see a rolling log of information describing what's going on in the background such as the subdivided quad renderings and such.

	$ bin/Mandela.sh
	2014-06-10 14:04:38.950 mono[99034:507] *** WARNING: Method userSpaceScaleFactor in class NSView is deprecated on 10.7 and later. It should not be used in new applications. Use convertRectToBacking: instead.
	[2:04:39 PM]Loading and starting Mandela
	[2:04:39 PM]Renderer : *** Visually Hinted Image Computation Beginning ***
	[2:04:39 PM]Renderer : Mandelbrot XMin[-2] XMax[1] YMin[-1] YMax[1] Screen_Color_Count[500] Iterations[100]
	[2:04:39 PM]Renderer :  --> Viewport Cleared
	[2:04:39 PM]Renderer :  --> Quad 0,0 Done [162500/650000]
	[2:04:39 PM]Renderer :  --> Subquad 1,1 Done [325000/650000]
	[2:04:39 PM]Renderer :  --> Subquad 1,0 Done [487500/650000]
	[2:04:39 PM]Renderer :  --> Subquad 0,1 Done [650000/650000]
	[2:04:39 PM]Renderer : *** Image Computation Complete (922ms) ***

![Mandela Interface](https://github.com/stevenvelozo/mandela/raw/master/screens/Mandela.png)

## Using the Application

You can click and mouse wheel around a little box and zoom in on the projection of the fractal.  Once you settle on a region you want to zoom into, you can right click (or use the Render menu).  You can change to a julia set, and a silly debug grid.  If you want to change colors, you change code.

![Mandela Zoom Interface](https://github.com/stevenvelozo/mandela/raw/master/screens/Mandela-Zoomer.png)

![Mandela Zoomed](https://github.com/stevenvelozo/mandela/raw/master/screens/Mandela-Zoomed.png)

## A Note on the "quality" Settings

There is a mechanism to adjust quality, and it does indeed make the images look better by simple supersampling of pixels.  It takes longer, in some cases going overboard with supersampling generates an image that is entirely TOO soft.  If you find a zoom you like and want to change quality, just change the quality setting in the Render menu and click the 'Render' entry at the top of the menu again.  High quality can take quite a bit of time.  The image below took over 2 minutes on my 2014 beefy macbook pro.

	[2:08:06 PM]Render Quality Changed to: 5
	[2:08:06 PM]Renderer : Quality Set To: 9
	[2:08:11 PM]Renderer : *** Visually Hinted Image Computation Beginning ***
	[2:08:11 PM]Renderer : Mandelbrot XMin[-1.205] XMax[-0.455] YMin[-0.493] YMax[-0.00700000000000001] Screen_Color_Count[500] Iterations[100]
	[2:08:11 PM]Renderer :  --> Viewport Cleared
	[2:08:45 PM]Renderer :  --> Quad 0,0 Done [162500/650000]
	[2:09:19 PM]Renderer :  --> Subquad 1,1 Done [325000/650000]
	[2:09:53 PM]Renderer :  --> Subquad 1,0 Done [487500/650000]
	[2:10:26 PM]Renderer :  --> Subquad 0,1 Done [650000/650000]
	[2:10:26 PM]Renderer : *** Image Computation Complete (135752ms) ***

![Mandela Zoom Interface](https://github.com/stevenvelozo/mandela/raw/master/screens/Mandela-HQ.png)
