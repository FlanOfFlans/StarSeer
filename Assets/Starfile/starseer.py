import re

# This script compiles all the data from the Bright Star Catalogue.
# For ease of other data processing, ALL data is compiled; however,
# for the time being, only relevant data (position, magnitude, color)
# are recorded.

class CelestialBody:

	temp_color_dict = {}

	# Read from TempToColor.dat, to compile to a dict for lookup.
	# TempToColor.dat was made by Mitchell Charity, and 
	# graciously provided online.
	pattern = re.compile(r"(\d?\d{4}) K   2deg.* ([\d ]{2}\d) ([\d ]{2}\d) ([\d ]{2}\d)  #")
	with open("TempToColor.dat") as f:
		for line in f.readlines():
			if line.startswith("#"): continue

			m = pattern.search(line)
			if m:
				temp = int(m.group(1))
				r = int(m.group(2))
				g = int(m.group(3))
				b = int(m.group(4))

				temp_color_dict[temp] = (r, g, b)

	def __init__(self):
		self.harvard_id = None
		self.name = None
		self.durchmusterung_id = None
		self.henry_draper_id = None
		self.sao_id = None
		self.fk5_id = None
		self.infrared = False
		self.infrared_code = None
		self.multiple_star_code = None
		self.ads_designation = None
		self.ads_component_count = None
		self.variable_star_designation = None
		self.b1900_arc_coordinates = None
		self.j2000_arc_coordinates = None
		self.galactic_longitude = None
		self.galactic_latitude = None
		self.visual_magnitude = None
		self.visual_magnitude_code = None
		self.visual_magnitude_uncertainty = None
		self.color_BV = None
		self.color_BV_uncertainty = None
		self.color_UB = None
		self.color_UB_uncertainty = None
		self.color_RI = None
		self.color_RI_system = None
		self.spectral_type = None
		self.spectral_type_code = None
		self.proper_motion_RA = None # Using J2000
		self.proper_motion_DEC = None # Using J2000
		self.parallax_type = "Trigonometric"
		self.parallax_arctime = None
		self.heliocentric_radial_velocity = None
		self.radial_velocity_comments = None
		self.rotational_velocity_limits = None
		self.rotational_velocity = None
		self.rotational_velocity_uncertainty = False
		self.magnitude_difference = None # Only applicable to multiples
		self.component_separation = None # Only applicable to occultation binaries
		self.magnitude_difference_components = None # Only applicable to multiples
		self.component_count = 1
		self.has_notes = False

	def __str__(self):
		final_string = ""			

		ids = []
		def append_if_exists(xlist, x, xstr): # Small helper mutation to reduce repetition
			if x != None:
				xlist.append(xstr.format(x))

		append_if_exists(ids, self.name, "{0}")
		append_if_exists(ids, self.harvard_id, "Harvard ID: {0}")
		append_if_exists(ids, self.durchmusterung_id, "DM: {0}")
		append_if_exists(ids, self.henry_draper_id, "HD: {0}")
		append_if_exists(ids, self.sao_id, "SAO: {0}")
		append_if_exists(ids, self.fk5_id, "FK5: {0}")
		append_if_exists(ids, self.ads_designation, "ADS: {0}")

		final_string += ", ".join(ids) + "\n"

		if self.ads_component_count != None:
			final_string += "Multiple star with {0} components.\n".format(self.ads_component_count)

		if self.visual_magnitude != None:
			final_string += "Visual magnitude: {0}\n".format(self.visual_magnitude)

		if self.j2000_arc_coordinates != None:
			final_string += str(self.j2000_arc_coordinates) + "\n"
		else:
			final_string += "This object is not a star.\n"

		return final_string

	def get_temp(self):
		return 7000 / (self.color_BV + 0.56)

	def get_color(self):
		t = self.get_temp()

		if t < 1000: t = 1000
		elif t > 40000: t = 40000

		t = int(100 * round(t/100))

		return CelestialBody.temp_color_dict[t]
		

class ArcCoordinate:
	def __init__(self, RA_hour, RA_min, RA_sec, sign, deg, DEC_min, DEC_sec):
		if RA_hour == None: # If the object didn't have coordinates
			raise ValueError

		self.RA_hour = RA_hour
		self.RA_min = RA_min
		self.RA_sec = RA_sec
		self.sign = sign
		self.deg = deg
		self.DEC_min = DEC_min
		self.DEC_sec = DEC_sec

	def __str__(self):
		return "RA {0} hr, {1} min, {2} sec, DEC {3}{4}°, {5}', {6}\"".format(
			self.RA_hour, self.RA_min, self.RA_sec,
			self.sign, self.deg, self.DEC_min, self.DEC_sec)

	def __repr__(self):
		return "{0}:{1}:{2} {3}{4}:{5}:{6}".format(
			self.RA_hour, self.RA_min, self.RA_sec,
			self.sign, self.deg, self.DEC_min, self.DEC_sec)

def load_celestial_body_from_line(line):
	process_string = lambda x: (x.strip() if x.strip() != "" else None)
	process_int = lambda x: (int(x.strip()) if x.strip() != "" else None)
	process_float = lambda x: (float(x.strip()) if x.strip() != "" else None)

	# Simple cases
	cb = CelestialBody()
	cb.harvard_id = int(line[0:4].strip())
	cb.name = process_string(line[4:14])
	cb.henry_draper_id = process_int(line[25:31])
	cb.sao_id = process_int(line[31:37])
	cb.fk5_id = process_int(line[37:41])
	cb.infrared = bool(line[41])
	cb.infrared_code = process_string(line[42])
	cb.multiple_star_code = process_string(line[43])
	cb.ads_designation = process_int(line[44:49])
	cb.ads_component_count = process_string(line[49:51])
	cb.variable_star_designation = process_string(line[51:60])
	cb.galactic_longitude = process_float(line[90:96])
	cb.galactic_latitude = process_float(line[96:102])
	cb.visual_magnitude = process_float(line[102:107])
	cb.visual_magnitude_code = process_string(line[107])
	cb.visual_magnitude_uncertainty = process_string(line[108])
	cb.color_BV = process_float(line[109:114])
	cb.color_BV_uncertainty = process_string(line[114])
	cb.color_UB = process_float(line[115:120])
	cb.color_UB_uncertainty = process_string(line[120])
	cb.color_RI = process_float(line[121:126])
	cb.color_RI_system = process_string(line[126])
	cb.spectral_type = process_string(line[127:147])
	cb.spectral_type_code = process_string(line[147])
	cb.proper_motion_RA = process_float(line[148:154])
	cb.proper_motion_DEC = process_float(line[154:160])
	cb.parallax_type = "Dynamical" if line[160] == "D" else "Trigonometric"
	cb.parallax_arctime = process_float(line[161:166])
	cb.heliocentric_radial_velocity = process_int(line[166:170])
	cb.radial_velocity_comments = process_string(line[170:174])
	cb.rotational_velocity_limits = process_string(line[174:176])
	cb.rotational_velocity = process_int(line[176:179])
	cb.rotational_velocity_uncertainty = process_string(line[179])
	cb.magnitude_difference = process_float(line[180:184])
	cb.component_separation = process_float(line[184:190])
	cb.magnitude_difference_components = process_string(line[190:194])
	cb.component_count = process_int(line[194:196])
	cb.has_notes = bool(line[196].strip())

	# Cases that require more processing
	full_dm = process_string(line[14:25])
	if cb.durchmusterung_id != None:
		cb.durchmusterung_id = "{0} {1}".format(full_dm[0:5], full_dm[5:].strip())

	try:
		cb.b1900_arc_coordinates = ArcCoordinate(
			process_int(line[60:62]), process_int(line[62:64]), process_float(line[64:68]),
			line[68], process_int(line[69:71]), process_int(line[71:73]), process_int(line[73:75]))
		cb.j2000_arc_coordinates = ArcCoordinate(
			process_int(line[75:77]), process_int(line[77:79]), process_float(line[79:83]),
			line[83], process_int(line[84:86]), process_int(line[86:88]), process_int(line[88:90]))
	except ValueError:
		cb.b1900_arc_coordinates = None
		cb.j2000_arc_coordinates = None

	return cb


print("Reading bodies into memory...")

bodies = []
with open("bsc5.dat") as f:
	lines = f.readlines()
	for line in lines:
		line = line.ljust(197)
		bodies.append(load_celestial_body_from_line(line))

print("Done!")
print("Writing bodies to disk...")

skipped_bodies = 0
with open ("starfile.txt", "w") as f:
	for body in bodies:
		try:
			line = "{0}::::{1}::::{2}\n".format(
				repr(body.j2000_arc_coordinates), 
				repr(body.get_color()),
				body.visual_magnitude)
			f.write(line)

		except:
			# If something doesn't have the requisite information, note it and continue.
			print("Object with Harvard ID {0} (name: {1}) lacks necessary information. This may be because it's not a star. Skipping." \
				.format(body.harvard_id, body.name))
			skipped_bodies += 1

print("Stars written!")
print("{0} bodies skipped.".format(skipped_bodies))