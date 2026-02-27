init
{
	$SHZ 10000;
	$SODX 65 66 67 68 69 257 83;
}

fn main(scanFreq=10000)
{
	//exposure(567, 80)
	adaptiveExposure(345, 54)
	//doubleExposure(456, 70, 10)
	avd(1)
	avs(1)
	enc(2, 0, 124)
	
	rect(x0=-30.1230, y0=-30.2281, x1=30.1230, y1=30.1544, nCols=16, nRows=16, interp=0, label="my_rect")
}

