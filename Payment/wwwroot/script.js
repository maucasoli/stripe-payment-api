const stripe = Stripe("pk_test_51SI0ZeIhKWD6wVbzqBlGiAchihg61Upe2ghGmvE7Rm5vO9GXIor3fpjWPwGMjo0WUb2XJeWzP6bgjWjgRGjlxR5V00W4D9Ge2z");

const elements = stripe.elements();
const cardElement = elements.create("card");
cardElement.mount("#card-element");

const form = document.getElementById("payment-form");
const resultDiv = document.getElementById("result");

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    resultDiv.textContent = "Processing...";

    const amount = parseFloat(document.getElementById("amount").value);
    const description = document.getElementById("description").value;

    const response = await fetch("https://localhost:7263/api/payments/stripe", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            amount,
            currency: "usd",
            description
        })
    });

    const data = await response.json();
    if (!response.ok || !data.clientSecret) {
        resultDiv.textContent = `Error: ${data.message || "Payment failed."}`;
        return;
    }

    // payment confirmation
    const { error, paymentIntent } = await stripe.confirmCardPayment(data.clientSecret, {
        payment_method: {
            card: cardElement,
        },
    });

    if (error) {
        resultDiv.textContent = "Error: " + error.message;
    } else if (paymentIntent.status === "succeeded") {
        resultDiv.textContent = "Payment approved!";
    } else {
        resultDiv.textContent = "Payment pending...";
    }
});
