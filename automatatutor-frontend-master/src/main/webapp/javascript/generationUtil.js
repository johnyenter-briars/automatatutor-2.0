function updateGenerationForm() {
	var mode = document.getElementById("modeSelect").value;
	var qualDiv = document.getElementById("quality_input");
	var diffDiv = document.getElementById("difficulty_input");
	
	var qualVisible = true;
	var diffVisible = true;
	
	if (mode === "best in difficutly range") {
		qualVisible = false;
	}
	else if (mode === "hardest") {
		diffVisible = false;
	}
	else if (mode === "any") {
		diffVisible = false;
	}
	
	if (qualVisible) qualDiv.style.display = "block";
	else qualDiv.style.display = "none";
	if (diffVisible) diffDiv.style.display = "block";
	else diffDiv.style.display = "none";
}