/**
 * Module for the Controller.
 *
 * @author Erik Lindholm <elimk06@student.lnu.se>
 * @author Mats Loock
 * @version 1.0.0
 */

import fetch from 'node-fetch'
import crypto from 'crypto'

/**
 * Encapsulates a controller.
 */
export class Controller {
  /**
   * Displays the index page.
   *
   * @param {object} req - Express request object.
   * @param {object} res - Express response object.
   * @param {Function} next - Express next middleware function.
   */
  async index (req, res, next) {
    console.log('Called controller.index')
    try {
      res.render('main/index')
    } catch (error) {
      next(error)
    }
  }

  async findTopMatchingUsers (req, res, next) {
    res.setHeader('Content-Type', 'application/json');
    res.writeHead(200)
    res.end(JSON.stringify(req.body))
  }
}
