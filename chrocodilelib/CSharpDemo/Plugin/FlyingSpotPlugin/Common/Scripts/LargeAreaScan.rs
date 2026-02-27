
init
{
   $SHZ 70000;
   $SCAN 11 0 0;
   $SODX 65, 66, 69, 82, 256;
   $LAI 80;
}

let factor = 1	// scaling factor for debugging
let areaW = 310/factor, areaH = 310/factor, // the desired scan area size in millimiters
    tolerance = 1,       	  // +/- area enlargement in each dimension (s.t., realAreaW/H is divisible by rasterSz)
	realAreaW = areaW + tolerance*2, 
	realAreaH = areaH + tolerance*2,
    rasterSz = 0.03			  // raster (or pixel) size in millimiters

// rectangle boundaries for the scan
let x1 = -realAreaW/2, y1 = -realAreaH/2,
	x2 =  realAreaW/2, y2 =  realAreaH/2
			
let resX = (x2 - x1) / rasterSz,	// total bitmap resolution needed
	resY = (y2 - y1) / rasterSz

fn main(scanFreq=70000)
{
	let ncols=resX, nrows=resY
	//correction(256, -1, 1)
	rect(x1, y1, x2, y2, nCols=ncols, nRows=nrows, interp=1, innerRad=5, outerRad=5, label="area", waitAtBegin=20000, waitAtEnd=2000)
}
