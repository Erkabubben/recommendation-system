async function postdata() {
    const formData = {
        user: document.querySelector('#users').value,
        similarity: document.querySelector('#similarity').value,
        results: document.querySelector('#results').value
    }
    console.log(formData)
    console.log(JSON.stringify(formData))
    const formDataString = await JSON.stringify(formData)
    const response = await fetch("./findTopMatchingUsers", {
        method: 'post',
        body: formDataString,
        headers: {
            'Content-Type': 'application/json'
          }
    })
    const responseJSON = await response.json()
    console.log(responseJSON)
    alert('Posted using Fetch: ' + responseJSON.user)
}

document.querySelector('#button-find-matching-users').addEventListener('click', postdata)
