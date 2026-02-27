
init
{
   $SHZ 70000;
   $SCAN 11 0 0;
   $SODX 65, 66, 69, 82, 256;
   $LAI 80;
}

fn main(scanFreq=70000)
{
	rect(-155, -155, 155, 155, nCols=10000, nRows=10000, interp=1, innerRad=5, outerRad=5, label="area", waitAtBegin=20000, waitAtEnd=2000)
}
