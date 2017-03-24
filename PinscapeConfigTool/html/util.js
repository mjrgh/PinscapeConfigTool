// "show" buttons, to show divs that are initially hidden.
// Set button class="shower".  Set the div class="hider".
// The "shower" and "hider" elements must be children of
// the same parent.
$(function() {
	$(".shower").click(function(ev)
	{
		// hide myself (the "show" button)
		var shower = $(this);
		shower.hide();

		// get the hider - the .hider element from the nearest enclosing
		// parent who has one
		var d = $(".hider", $(this).parents(":has(.hider)").first()).first();

		// slide it open
		slideOpen(d, ev.originalEvent ? 250 : 0, true);

		if (!shower.hasClass("noReHide") && d.find(".reHideButton").length == 0) {
			var rehider = $("<a href=\"#\" class=\"reHideButton\">Hide these details</a>")
						  .click(function(ev) {
				ev.preventDefault();
				shower.show();
				slideClosed(d, 250);
				rehider.detach();
			});
			d.append(rehider);
		}

		// don't process the href click
		ev.preventDefault();		
	});
});

// Global javascript error handling
$(function() {
	window.onerror = function(msg, url, line, col, error)
	{
		// set up the detail message
		var loc = (/\/(html\/[^?]+)/.test(url) ? RegExp.$1 : loc);
		var details = loc + ", line " + line
					  +     (col !== undefined ? ", col " + col : "")
					  + "\r\n" + error
					  + (/.*:/.test(error) ? "" : msg);
		
		// check for old IE versions
		var vsn = window.external.WebControlVersion();
		/(\d+)\.(\d+)/.test(vsn);
		var vsnMajor = parseInt(RegExp.$1);
		var vsnMinor = parseInt(RegExp.$2);
		if (vsnMajor < 11)
		{
			// IE is older than IE 11 - could be the problem
			msg = "Internal error!\r\n"
				  + "An error has occurred within the config tool. "
				  + "You have an older version of Internet Explorer "
				  + "installed on your system (IE " + vsnMajor + "."
				  + vsnMinor + "), which could be the source of the "
				  + "problem.  This program requires IE 11 or newer "
				  + "to be installed.  (This is required even if you "
				  + "don't use IE as your browser, since the program "
				  + "uses components of IE internally.)"
				  + "\r\n\r\n"
				  + "Please update IE to the latest version.  You can "
				  + "download it from the Microsoft Web site or use "
				  + "Windows Update.";
		}
		else
		{
			// IE seems up to date, so we apparently can't point fingers
			msg = "Internal error!\r\n"
				  + "An error has occurred within the config tool. "
				  + "This is probably due to a bug in the program, not "
				  + "anything you did wrong.\r\n\r\n"
				  + "Technical details (for debugging):\r\n"
				  + details;
		}

		window.external.Alert(msg);
		return true;
	};
});

// Slide open an item hidden by "height: 0px" in the CSS
function slideOpen(dd, t, scroll)
{
	$(dd).each(function() {
		// figure the element's natural height by temporarily setting
		// the height to "auto", then setting it back to 0px (the height
		// when hidden)
		var d = $(this);
		if ($(d).stop().height() == 0)
		{
			// set to natural height so we can measure it
			var ht = d.css("height", "auto").height();
			if (t === 0)
			{
				// no animation - scroll into view if desired
				if (scroll)
					scrollIntoView(d);
			}
			else
			{
				// set it back to 0px to start the animation
				d.css("height", "0px");
				
				// Now animate changing the height to the measured height.
				// When done, set the height to "auto" rather than the
				// measured height, to accommodate changes.
				d.animate({height: ht}, t || 250, "swing", function() {
					d.css("height", "auto");
					if (scroll)
						scrollIntoView(d);
				});
			}
		}
	});
}

// Slide an item closed, effectively hiding it, by setting
// its height to 0px
function slideClosed(dd, t)
{
	$(dd).each(function()
	{
		// If it's already at zero height, or the animation
		// time is zero, skip the animation.
		var d = $(this);
		d.css("overflow", "hidden");
		if ($(d).stop().height() == 0 || t === 0)
		{
			// no animation - just set the height immediately
			d.css("height", "0px");
		}
		else
		{
			// animate the height going to zero
			d.animate({height: 0}, t || 250, "swing");
		}
	});
}

// Toggle an item open or closed
function slideToggle(dd, open, t, scroll)
{
	$(dd).each(function() {
		var d = $(this);
		if (open)
			slideOpen(d, t, scroll);
		else
			slideClosed(d, t);
	});
}

// Slide open only one of a "radio button" group of items,
// closing all of the others.
function slideRadio(open, all, t, scroll)
{
	// close all items except the open one
	slideClosed(all.filter(function() { return !$(this).is(open); }), t);

	// open the opening item
	slideOpen(open, t, scroll);
}


// "Better" scrollIntoView.  Unlike the system version, this
// one only scrolls if necessary to bring the whole object
// into view, and scrolls the minimal amount to do so.  We
// also will scroll items within scrolling containers rather
// than the whole window.
//
// d is the element to scroll into view
// t is the time delay; undefined uses a default
// adjust is an object with adjustments to the scrolling
//    area: top, left, bottom, right, expressed as pixel
//    INSET amounts.  For example, top:8 means that the
//    top of the scrolling window is 8 pixels below the
//    actual element top; bottom:8 means that the bottom
//    is 8 pixels above the element buttom.
function scrollIntoView(d, t, adjust)
{
	// Do the whole scrolling setup in a timeout, to allow any
	// deferred rendering actions to complete before we start
	// measuring anything.
	setTimeout(function()
	{
		// set up an empty adjustments object by default
		adjust = adjust || { }
		var topAdjust = adjust.top || 0;
		var leftAdjust = adjust.left || 0;
		var htAdjust = topAdjust + (adjust.bottom || 0);
		var widAdjust = leftAdjust + (adjust.right || 0);
		
		// make sure we have something to scroll into view
		if (d && d.length)
		{
			// apply a defualt time
			if (t != 0 && !t)
				t = 250;
			
			// get the object position and size
			var objy = d.offset().top;
			var objht = d.height();

			// add in some padding at the bottom for aesthetics
			objht += 16;

			// figure out what we're scrolling
			var par;
			d.parents().each(function() {
				var o = $(this).css("overflow");
				if (o == "auto" || o == "scroll") {
					par = $(this);
					return false;
				}
			});

			// if we found a scrolling parent, scroll within the
			// parent, otherwise scroll the whole window
			if (par)
			{
				// we have a scrolling parent - scroll within the container

				// get the exposed container area
				var scrolly = par.scrollTop();
				var contht = par.innerHeight() - htAdjust;

				// adjust the object position to be relative to the container
				objy -= (par.offset().top + topAdjust);

				// If the object is out of view above, scroll up.  If the
				// bottom is out of view, scroll so the bottom is in view,
				// but not so far that the top goes out of view.
				if (objy < 0)
				{
					// out of view above - scroll to the top
					par.stop().animate({ scrollTop: scrolly + objy }, t);
				}
				else if (objy + objht > contht)
				{
					// out of view below - scroll to the bottom
					var dy = objy + objht - contht;

					// if this would take the target out of view above, simply
					// scroll to put the target object at the top
					if (objy - dy < 0)
						dy = objy;

					// do the scrolling
					par.stop().animate({ scrollTop: scrolly + dy }, t);
				}
			}
			else
			{
				// get the exposed document area
				var scrolly = $("html,body").scrollTop();
				var winht = window.innerHeight;

				// deduct the nav bar, if present, and the height adjustment
				winht -= $("#nav").outerHeight() + htAdjust;
				
				// If the object is out of view above, scroll up.  If the bottom
				// is out of view, scroll so its bottom is in view, but don't
				// let its top go out of view.
				if (objy < scrolly + topAdjust)
				{
					// top out of view - scroll to top
					$("html, body").stop().animate({ scrollTop: objy - topAdjust }, t);
				}
				else if (objy + objht > scrolly + winht)
				{
					// figure top such that bottom is in view
					var newy = objy + objht - winht;
					
					// limit to the top position
					var topy = objy - winht;
					if (newy < topy) newy = topy;
					
					// scroll there
					$("html, body").stop().animate({ scrollTop: newy }, t);
				}
			}
		}
	}, 1);
}

// Convert a null object reference to an empty object reference,
// to simplify property evaluation in cases where the object
// is missing.
function nullobj(obj)
{
	return obj || { };
}

// Constrain an index to be within an array's bounds
function boundsCheck(val, array)
{
	if (val > array.length - 1) val = array.length - 1;
	if (val < 0) val = 0;
	return val;
}

// get the width of the browser scrollbars
var scrollBarWidth;
function getScrollBarWidth()
{
	// if we haven't already cached the width, calculate it
	if (scrollBarWidth === undefined)
	{
		// Compute the width with a probe division: create a div with
		// a fixed width and an explicit scrollbar, then create a child
		// of the first div.  The child will be sized to the interior
		// width of the outer div, which excludes its scrollbar.  In
		// other words, the inner div's actual layout width will be the
		// same as the outer div minus the scrollbar width.  Hence the
		// scrollbar width is the difference of the div widths.
		var outer = $("<div>").css({
			"visibility": "hidden",
			width: "100px",
			overflow: "scroll"
		}).appendTo("body");
		var innerWid = $("<div>").css({"width": "100%"}).appendTo(outer).outerWidth();
		outer.remove();
		scrollBarWidth = 100 - innerWid;
	}

	// return the cached value
	return scrollBarWidth;
}

// Show the result of a C# system call, using our internal result
// object format:
//
//  {
//    status: "ok" means the operation was successful, with a success message in .message
//            "error" means there's an error in .message
//            "cancel" means the user manually canceled via a dialog option
//    message: string with displayable result message
//  }
//
// 'handler' is an optional set of custom handlers.  This is a hash,
// keyed by status string ("ok", "error", etc), with values as functions
// taking the parsed result object as argument.
//
// Returns the parsed result object.
// 
function showCallResult(res, handler)
{
	// evaluate the string into a javascript object, and apply a default
	var o = (typeof res == "string" ? eval(res) : res);
	if (typeof o != "object")
		o = { status: "error", message: "Unrecognized response: " + res };

	// show an appropriate message
	var defHandler = {
		"cancel": function() { },   // user canceled - no more feedback required
		"error": function() { alert("An error occurred: " + o.message); },
		"ok": function() { if (o.message) alert(o.message); }
	};
	var func = (handler && handler[o.status])
			   || defHandler[o.status]
			   || function() { alert("Unknown result status: " + o.status); };
	func(o);

	// return the parsed object
	return o;
}

// Wait for the device to reboot.  This switches to the "wait for reboot"
// page, passing a link back to the originating page so that we can return
// here automatically when the device is back online.
function waitForReboot(cpuid)
{
	// it's not present - switch to the WaitForReboot page
	window.navigate("WaitForReboot.htm?ID=" + cpuid + "&return=" + encodeURIComponent(window.location.href));
	return false;
}


// HTMLify a string - escape special HTML characters
$(function() {
	String.prototype.htmlify = function() {
		return this.replace(/[&<>"'\/]/g, function(m) {
			return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&#34;", "'": "&#39;", "/": "&#47;" }[m];
		});
	};
});

// On load, vertically center #nav div items
//$(function() {
//	var nav = $("#nav");
//	var ht = nav.innerHeight();
//	nav.children().each(function() {
//		$(this).css("margin-top", ((ht - $(this).outerHeight())/2) + "px");
//	});
//});


// String.prototype.format()
// Returns a new string where the patterns $1, $2, etc are replaced
// with the corresponding arguments.  Use $$ for a literal '$'.
String.prototype.format = function(f)
{
	var a = arguments;
	return this.replace(/\$($|\d\d?)/g, function(m, p) {
		return p == "$" ? p : a[parseInt(p) - 1];
	});
}

// String.prototype.formatOrUndef()
// Works like format(), with the added feature that the overall
// result is 'undefined' if any of the referenced arguments are
// undefined.  Note that unreferenced arguments trigger no special
// behavior: "$1".formatOrUndef('one', undefined) returns "one",
// even though the unreferenced second argument is undefined.
String.prototype.formatOrUndef = function()
{
	var a = arguments;
	var undef = false;
	var s = this.replace(/\$($|\d\d?)/g, function(m, p)
	{
		// return literal '$' for '$$'
		if (p == "$")
			return p;

		// get the argument value
		var val = a[parseInt(p) - 1];

		// if it's undefined, note it in the results
		if (val === undefined)
			undef = true;

		// return the value
		return val;
	});

	// if any referenced arguments were undefined, return undefined,
	// otherwise return the result string
	return undef ? undefined : s;
}

// String.prototype.formatOrEmpty()
// Works like formatOrUndef(), but returns an empty string if
// any of the referenced arguments are undefined.
String.prototype.formatOrEmpty = function()
{
	return String.prototype.formatOrUndef.apply(this, arguments) || "";
}

window.alert = function(msg) {
	window.external.Alert(msg);
}