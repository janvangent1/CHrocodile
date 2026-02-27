init
{
	$SODX 256 257 260 82 68 69 65 66 67 ;
	$SHZ 50000;$LAI 7;$AVD 1;$AVS 1;$SRI 1;$ABE 0;$SRT 0;$NOP 1;$POD 0;$OFN 0 0;
    $MMD 1;
    $DWD 425,2400;
}

fn main(scanFreq=50000)
{
	adaptiveExposure(shz=50000,aal=7)
	
	// signal Z-correction is a persistent setting: it can be set only once
	// and remains unchanged until another 'correction' command 
	correction(signalID=260, zdir=-1, keepLinear=1)
	
	// iterate infinitely many times
	loop {
		// initial scanning position and step along the Y axis
		let X = -10, Y = 140, step = 5.75
		
		// scan the sequence of spirals evently distributed along the Y axis
		for i in range(49) {
			let y = Y - i*step
			spiral(x0=X, y0=Y, a=0, b=0.175070434871385, nTurns=3, numPts=3154, waitAtBegin=5000, label="Spiral")
		}
		
		// scan one more figure with "dummy" label indicating the end of one scan step
		// this also moves the scanner back to its original position (X, Y) ready for the next step
		ellipse(X, Y, radX=1, radY=1, nTurns=1, numPts=100, angle=0, waitAtBegin=100, label="dummy")
		
		// wait for a software trigger to start the next iteration
		waitTrigger()
	}
}