document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector("#reservationForm");
    if (!form) return;

    const stationId = form.querySelector("#GamingStationId");
    const dateInput = form.querySelector("#ReservationDate");
    const timeSelect = form.querySelector("#startTimeSelect");
    const message = form.querySelector("#availabilityMessage");
    const totalPrice = form.querySelector("#totalPrice");
    const durationInputs = [...form.querySelectorAll('input[name="DurationHours"]')];
    const hourlyRate = Number(form.dataset.hourlyRate || 0);

    const selectedDuration = () => Number(durationInputs.find(input => input.checked)?.value || 1);

    const updateTotal = () => {
        totalPrice.textContent = new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 })
            .format(hourlyRate * selectedDuration());
    };

    const loadAvailability = async () => {
        const date = dateInput.value;
        if (!date) {
            timeSelect.innerHTML = '<option value="">Choose a date first</option>';
            return;
        }

        timeSelect.disabled = true;
        timeSelect.innerHTML = '<option value="">Scanning available slots...</option>';
        message.textContent = "Checking live availability...";

        try {
            const query = new URLSearchParams({
                stationId: stationId.value,
                date,
                durationHours: selectedDuration().toString()
            });
            const response = await fetch(`/Reservations/Availability?${query}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            if (!response.ok) throw new Error("Availability request failed.");

            const slots = await response.json();
            timeSelect.innerHTML = '<option value="">Select a start time</option>';
            slots.forEach(slot => timeSelect.add(new Option(slot.label, slot.value)));
            message.textContent = slots.length
                ? `${slots.length} start time${slots.length === 1 ? "" : "s"} available.`
                : "No compatible start times remain. Try another date or duration.";
        } catch {
            timeSelect.innerHTML = '<option value="">Could not load times</option>';
            message.textContent = "Availability could not be loaded. Please retry.";
        } finally {
            timeSelect.disabled = false;
        }
    };

    durationInputs.forEach(input => input.addEventListener("change", () => {
        updateTotal();
        loadAvailability();
    }));
    dateInput.addEventListener("change", loadAvailability);
    updateTotal();
    loadAvailability();
});
