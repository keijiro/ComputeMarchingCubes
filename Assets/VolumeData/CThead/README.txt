
	Announcing the Chapel Hill Volume Rendering Test Data Set,
			   Volume II

		SoftLab Software Systems Laboratory
		University of North Carolina
		Department of Computer Science
		Chapel Hill, NC  27599-3175

The Chapel Hill Volume Rendering Test Data Set, Volume II is a collection
of the following files:

CT Cadaver Head data - A 113-slice MRI data set of a CT study of
a cadaver head.  Slices are stored consecutively as a 256 x 256 array
with dimensions of z-113 y-256 x-256 in z-y-x order.  Format is 16-bit
integers -- two consecutive bytes make up one binary integer.  14,811,136
bytes total file size.  Data taken on the General Electric CT Scanner and
provided courtesy of North Carolina Memorial Hospital.

CT Cadaver Head data information article - An ascii file containing
acknowledgements for the CT cadaver head data files.

MR Brain data -  A 109-slice MRI data set of a head with
skull partially removed to reveal brain.  256 x 256 array.
with dimensions of Z=109 Y=256 X=256 in z-y-x order.  Format is 16-bit
integers -- two consecutive bytes make up one binary integer. 14,286,848
bytes total file size.  Data taken on the Siemens Magnetom and provided
courtesy of Siemens Medical Systems, Inc., Iselin, NJ.  Data edited 
(skull removed) by Dr. Julian Rosenman, North Carolina Memorial Hospital.

MR Brain data information article - An ascii file containing acknowledgements
for the MR brain data files.

RNA data - A ASCII data set of an electron density map for
Staphylococcus Aureus Ribonuclease with space of (x,y,z) =
(0.94,0.94,0.94) and dimensions z-16, y-120, x-100 in z-y-x order.
961,920 bytes total file size.  Data provided courtesy of Dr. Chris Hill,
University of York.

RNA data information article - An ascii file containing acknowledgements
for the RNA data files.

The data sets were written on a Digital Equipment Corporation (DEC) VAX
computer.  Each file contains only pixels, stored in row major order
with 2-byte integers per pixel.  To use the images on machines that
have normal byte order (DECs use reverse byte order), you should swap
alternate bytes, for example using the 'dd' command in UNIX.  A sample
command that does this for the MRbrain data set is:
	% dd if=MRbrain of=MRbrain.new conv=swab

We do not object to your further distributing these files, but we
request that full acknowledgement of the source of the data accompany
such distribution.  If you are going to send a data set to someone,
please also send the accompanying information file (*.info) and this
file (Announcement).

The Computer Science Department, University of North Carolina only
distributes these files by anonymous FTP.

We do not provide any software for displaying these data.

There is no information available about the data provided here other
than that present in these files.  For example, missing information
about the means of data collection cannot be provided.
