# A1 - Recommendation System

## Description

* In this assignment, you shall implement a recommendation system for the movie's data set.
* You can use any programming language you like.
* You shall present your application and code at an oral examination.

## Submission instructions

See the [Deadlines and Submissions](https://coursepress.lnu.se/courses/web-intelligence/study-guide/deadlines-and-submissions) page.

## Requirements

<table>
  <tr>
    <th>Grade</th>
    <th>Requirements</th>
  </tr>
  <tr>
    <td>E</td>
    <td>
      <ul>
        <li>Build a recommendation system that can find similar users and find recommendations for a user, using the <em>movies large</em> dataset (see <a href="https://coursepress.lnu.se/courses/web-intelligence/assignments/datasets">Datasets</a> page).</li>
        <li>You can verify that your application works by using the example dataset from the lecture (see <a href="https://coursepress.lnu.se/courses/web-intelligence/assignments/datasets">Datasets</a> page).</li>
        <li>Use Euclidean distance as the similarity measure.</li>
        <li>Implement the system using a REST web service where:
          <ol>
            <li>client sends a request to a server</li>
            <li>the server responds with <em>json</em> data</li>
            <li>the <em>json</em> data is decoded and presented in a client GUI</li>
          </ol>
        </li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>C-D</td>
    <td>
    <ul>
      <li>Implement the Pearson Correlation similarity measure.</li>
      <li>It shall be possible to select which measure to use from the client GUI.</li>
    </ul>
    </td>
  </tr>
  <tr>
    <td>A-B</td>
    <td>
      <ul>
        <li>Implement functionality for pre-generating an Item-Based Collaborative Filtering table by transforming the original data set.</li>
        <li>Use the pre-generated table to implement a second way of finding recommendations for a user.</li>
        <li>You shall only use Euclidean distance as the similarity measure.</li>
        <li>It shall be possible to select how to find recommendations from the client GUI (Item-Based or User-Based).</li>
      </ul>
    </td>
  </tr>
</table>

## Test cases – small example dataset

Here are some test cases for the small example dataset you can use to verify that your system works correctly.

* Find recommended movies for user **Mike** using **Euclidean distance**:<br />![A1-small-Ex1.png](.readme/A1-small-Ex1.png)
* Find recommended movies for user **Mike** using **Pearson similarity**:<br />![A1-small-Ex2.png](.readme/A1-small-Ex2.png)
* Find recommended movies for user **Mike** using **Item-based filtering**:<br />![A1-small-Ex3.png](.readme/A1-small-Ex3.png)

## Test cases – large dataset

Here are some test cases for the larger dataset you can use to verify that your system works correctly.

* Find recommended movies for user **Angela** using **Euclidean distance**:<br />![A1-Ex1.png](.readme/A1-Ex1.png)
* Find recommended movies for user **Will** using **Pearson similarity**:<br />![A1-Ex2.png](.readme/A1-Ex2.png)
* Find recommended movies for user **Andy** using **Item-based filtering**:<br />![A1-Ex3.png](.readme/A1-Ex3.png)
